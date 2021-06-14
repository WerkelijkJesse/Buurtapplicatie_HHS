using Buurt_interactie_app_Semester3_WDPR.Areas.Identity.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Buurt_Test.Unit_tests
{
    public class MockSignInManager : SignInManager<BuurtAppUser>
    {
        public MockSignInManager()
                : base(new MockUserManager(),
                     new Mock<IHttpContextAccessor>().Object,
                     new Mock<IUserClaimsPrincipalFactory<BuurtAppUser>>().Object,
                     new Mock<IOptions<IdentityOptions>>().Object,
                     new Mock<ILogger<SignInManager<BuurtAppUser>>>().Object,
                     new Mock<IAuthenticationSchemeProvider>().Object,
                     new Mock<IUserConfirmation<BuurtAppUser>>().Object)
        { }
    }



    public class MockUserManager : UserManager<BuurtAppUser>
    {
        public MockUserManager()
            : base(new Mock<IUserStore<BuurtAppUser>>().Object,
              new Mock<IOptions<IdentityOptions>>().Object,
              new Mock<IPasswordHasher<BuurtAppUser>>().Object,
              new IUserValidator<BuurtAppUser>[0],
              new IPasswordValidator<BuurtAppUser>[0],
              new Mock<ILookupNormalizer>().Object,
              new Mock<IdentityErrorDescriber>().Object,
              new Mock<IServiceProvider>().Object,
              new Mock<ILogger<UserManager<BuurtAppUser>>>().Object)
        { }

        public override Task<IdentityResult> CreateAsync(BuurtAppUser user, string password)
        {
            return Task.FromResult(IdentityResult.Success);
        }

        public override Task<IdentityResult> AddToRoleAsync(BuurtAppUser user, string role)
        {
            return Task.FromResult(IdentityResult.Success);
        }

        public override async Task<IList<string>> GetRolesAsync(BuurtAppUser user)
        {
            return new List<string>{ "Default" };
        }

        public override Task<string> GenerateEmailConfirmationTokenAsync(BuurtAppUser user)
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }

    }
}
