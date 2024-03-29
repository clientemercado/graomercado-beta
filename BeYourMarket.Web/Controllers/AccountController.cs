﻿using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using BeYourMarket.Web.Models;
using BeYourMarket.Web.Utilities;
using BeYourMarket.Service;
using BeYourMarket.Service.Models;
using i18n;
using BeYourMarket.Model.Enum;
using BeYourMarket.Model.Models;
using Repository.Pattern.UnitOfWork;

namespace BeYourMarket.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        #region Fields
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IEmpresaUsuarioService _empresaUsuarioService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        private readonly ICidadesService _cidadesService;
        private readonly IEstadoService _estadoService;
        private readonly DataCacheService _dataCacheService;
        private readonly SqlDbService _sqlDbService;
        #endregion

        string idUsu = "";

        #region Properties
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? System.Web.HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? System.Web.HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
        #endregion

        #region Constructor
        public AccountController(
            IUnitOfWorkAsync unitOfWorkAsync,
            IEmailTemplateService emailTemplateService,
            IEmpresaUsuarioService empresaUsuarioService,
            ICidadesService cidadesService,
            IEstadoService estadoService,
            DataCacheService dataCacheService,
            SqlDbService sqlDbService)
        {
            _emailTemplateService = emailTemplateService;
            _empresaUsuarioService = empresaUsuarioService;
            _cidadesService = cidadesService;
            _estadoService = estadoService;
            _unitOfWorkAsync = unitOfWorkAsync;
            _dataCacheService = dataCacheService;
            _sqlDbService = sqlDbService;
        }
        #endregion

        #region Methods

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl, int? oqeq)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.oqeq = (oqeq >= 0) ? oqeq : 0;

            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl, int? oqeq)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.oqeq = (oqeq >= 0) ? oqeq : 0;

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            // Require the user to have a confirmed email before they can log on.
            if (CacheHelper.Settings.EmailConfirmedRequired)
            {
                var user = await UserManager.FindByNameAsync(model.Email);                
                if (user != null)
                {
                    var roleAdministrator = await RoleManager.FindByNameAsync(Enum_UserType.Administrator.ToString());
                    var isAdministrator = user.Roles.Any(x => x.RoleId == roleAdministrator.Id);

                    // Skip email check unless it's an administrator
                    if (!isAdministrator && !await UserManager.IsEmailConfirmedAsync(user.Id))
                    {
                        ModelState.AddModelError("", "[[[Você deve confirmar antes o seu e-mail!]]]");
                        return View(model);
                    }
                }
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        //return RedirectToAction("Index", "Manage");
                        return RedirectToAction("Index", "Manage", new { oqeq = oqeq });
                    }
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "[[[Login ou Senha inválido(s).]]]");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "[[[Invalid code.]]]");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            var model = new RegisterViewModel()
            {
                TiposCadastro = CacheHelper.TiposCadastro,
                EstadosUF = CacheHelper.EstadoUf.OrderBy(e => (e.CODIGO)).ToList()
            };

            return View(model);
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await RegisterAccount(model);

                // Add errors
                AddErrors(result);
                if (result.Succeeded)
                {
                    if (Convert.ToInt32(model.tipoCad) == 1)
                    {
                        return RedirectToAction("CompanyData", "Manage");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Manage");
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IdentityResult> RegisterAccount(RegisterViewModel model)
        {
            // Consultar ID do ESTADO e CIDADE
            var cidade = CacheHelper.Cidade.FirstOrDefault(e => (e.NOME.ToUpper() == model.CidadeUF.ToUpper()));
            var estado = CacheHelper.EstadoUf.FirstOrDefault(e => (e.SIGLA.ToUpper() == model.EstadoUF.ToUpper()));

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = (model.FirstName != null) ? model.FirstName : model.Usuario,
                LastName = (model.LastName != null) ? model.LastName : "",
                RegisterDate = DateTime.Now,
                RegisterIP = System.Web.HttpContext.Current.Request.GetVisitorIP(),
                LastAccessDate = DateTime.Now,
                LastAccessIP = System.Web.HttpContext.Current.Request.GetVisitorIP(),
                id_TipoCadastro = Convert.ToInt32(model.tipoCad),
                id_UF = Convert.ToInt32(estado.ID),
                id_Cidade = Convert.ToInt32(cidade.ID),
                //Logradouro_Cidade = model.inLogradouro,
                //Bairro_Cidade = model.inBairro,
                //Cep_Bairro_Cidade = model.inCep
            };

            var result = await UserManager.CreateAsync(user, model.Password);
            idUsu = user.Id;

            if (result.Succeeded)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                // Send Message
                var roleAdministrator = await RoleManager.FindByNameAsync(BeYourMarket.Model.Enum.Enum_UserType.Administrator.ToString());
                var administrator = roleAdministrator.Users.FirstOrDefault();

                var message = new MessageSendModel()
                {
                    UserFrom = administrator.UserId,
                    UserTo = user.Id,
                    Subject = HttpContext.ParseAndTranslate(string.Format("[[[Olá, Seja bem-vindo ao {0}!]]]", CacheHelper.Settings.Name)),
                    Body = HttpContext.ParseAndTranslate(string.Format("[[[Olá, Seja bem-vindo ao {0}! Ficaremos felizes em ajudá-lo se você tiver alguma dúvida.]]]", CacheHelper.Settings.Name))

                };

                await MessageHelper.SendMessage(message);

                //// Send an email with this link
                //string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);

                //var urlHelper = new UrlHelper(System.Web.HttpContext.Current.Request.RequestContext);
                //var callbackUrl = urlHelper.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: System.Web.HttpContext.Current.Request.Url.Scheme);

                var emailTemplateQuery = await _emailTemplateService.Query(x => x.Slug.ToLower() == "signup").SelectAsync();
                var emailTemplate = emailTemplateQuery.FirstOrDefault();

                if (emailTemplate != null)
                {
                    dynamic email = new Postal.Email("Email");
                    email.To = user.Email;
                    email.From = CacheHelper.Settings.EmailAddress;
                    email.Subject = emailTemplate.Subject;
                    email.Body = emailTemplate.Body;
                    email.CallbackUrl = callbackUrl;
                    EmailHelper.SendEmail(email);
                }
            }            

            return result;
        }

        //private async Task<EmpresaUsuario> RegisterCompany(string nomeEmpresa)
        //{
        //    //OBS: CONTINUAR AQUI...  1- Testar a gravação sem referenciar o campo ID do Usuário, que já foi removido da re-criação da tabela 'EmpresaUsuario';
        //    //                        2- Se der certo, criar o campo 'Id_Empresa' na tabela 'AspNetUsers' e gravar o Id da Empresa criada;

        //    var novaEmpresa = new EmpresaUsuario()
        //    {
        //        Data_Cadastro_Empresa = DateTime.Now,
        //        Cnpj_Empresa = null,
        //        Razao_Social_Empresa = null,
        //        Logo_Empresa = null,
        //        Logradouro_Empresa = null,
        //        Complemento_Endereco_Empresa = null,
        //        Bairro_Empresa = null,
        //        Cidade_Empresa = null,
        //        UF_Empresa = null,
        //        Fone1_Empresa = null,
        //        Fone2_Empresa = null,
        //        Email1_Empresa = null,
        //        Email2_Empresa = null,
        //        Receber_Emails_Empresa = true
        //    };

        //    //novaEmpresa.Id = idUsu;
        //    novaEmpresa.Fantasia_Empresa = nomeEmpresa;

        //    _empresaUsuarioService.Insert(novaEmpresa);
        //    await _unitOfWorkAsync.SaveChangesAsync();

        //    return novaEmpresa;
        //}


        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // Check if email confirm required
                if (CacheHelper.Settings.EmailConfirmedRequired && !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Redefina sua senha clicando <a href=\"" + callbackUrl + "\">aqui</a>");
                //return RedirectToAction("ForgotPasswordConfirmation", "Account");

                //string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                //var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                var emailTemplateQuery = await _emailTemplateService.Query(x => x.Slug.ToLower() == "forgotpassword").SelectAsync();
                var emailTemplate = emailTemplateQuery.Single();

                dynamic email = new Postal.Email("Email");
                email.To = user.Email;
                email.From = CacheHelper.Settings.EmailAddress;
                email.Subject = emailTemplate.Subject;
                email.Body = emailTemplate.Body;
                email.CallbackUrl = callbackUrl;
                EmailHelper.SendEmail(email);

                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View(new ResetPasswordViewModel() { Code = code });
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            Session.Abandon();
            Response.Cookies.Clear();

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        /// <summary>
        /// Carrega a lista de Cidades
        /// </summary>
        /// <param name="idUf">String contendo a UF</param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult CarregaCidadesUF(int idUf)
        {
            var listaCidades = CacheHelper.Cidade.Where(c => (c.FK_ESTADO == idUf)).ToList();
            return Json(listaCidades, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
        #endregion
    }
}