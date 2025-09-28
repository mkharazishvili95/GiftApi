using FluentAssertions;
using FluentValidation.TestHelper;
using GiftApi.Application.User.Commands.Register;
using GiftApi.Application.User.Validators;
using GiftApi.Infrastructure.Data;
using GiftApi.Tests.Helpers;

namespace GiftApi.Tests.User
{
    [TestFixture]
    public class RegisterUserValidatorTests
    {
        private ApplicationDbContext _db;
        private RegisterUserValidator _validator;

        [SetUp]
        public void Setup()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _validator = new RegisterUserValidator(_db);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        private RegisterUserCommand GetValidCommand()
        {
            return new RegisterUserCommand
            {
                FirstName = "Misho",
                LastName = "Kharazishvili",
                Password = "Secret123",
                ConfirmPassword = "Secret123",
                IdentificationNumber = "12345678901",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                LoginType = Common.Enums.User.LoginIdentifierType.PhoneNumber,
                PhoneNumber = "+995599111111",
                Email = "dummy@example.com",
                Gender = Common.Enums.User.Gender.Male
            };
        }

        [Test]
        public async Task Should_Have_Error_When_FirstName_Is_Empty()
        {
            var command = GetValidCommand();
            command.FirstName = "";
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(c => c.FirstName);
        }

        [Test]
        public async Task Should_Have_Error_When_LastName_Is_Empty()
        {
            var command = GetValidCommand();
            command.LastName = "";
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(c => c.LastName);
        }

        [Test]
        public async Task Should_Have_Error_When_Password_Too_Short()
        {
            var command = GetValidCommand();
            command.Password = "123";
            command.ConfirmPassword = "123";
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(c => c.Password);
        }

        [Test]
        public async Task Should_Have_Error_When_ConfirmPassword_DoesNotMatch()
        {
            var command = GetValidCommand();
            command.ConfirmPassword = "Mismatch";
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(c => c.ConfirmPassword);
        }

        [Test]
        public async Task Should_Have_Error_When_User_Is_Under18()
        {
            var command = GetValidCommand();
            command.DateOfBirth = DateTime.UtcNow.AddYears(-15);
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(c => c.DateOfBirth);
        }

        [Test]
        public async Task Should_Have_Error_When_LoginType_Email_But_Email_Invalid()
        {
            var command = GetValidCommand();
            command.LoginType = Common.Enums.User.LoginIdentifierType.Email;
            command.Email = "invalid-email";
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(c => c.Email);
        }

        [Test]
        public async Task Should_Have_Error_When_IdentificationNumber_Invalid()
        {
            var command = GetValidCommand();
            command.IdentificationNumber = "12345";
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(c => c.IdentificationNumber);
        }

        [Test]
        public async Task Should_Have_Error_When_PhoneNumber_Invalid()
        {
            var command = GetValidCommand();
            command.PhoneNumber = "abc123";
            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(c => c.PhoneNumber);
        }

        [Test]
        public async Task Should_Not_Have_Error_When_Command_Is_Valid()
        {
            var command = GetValidCommand();
            var result = await _validator.TestValidateAsync(command);
            result.IsValid.Should().BeTrue();
        }
    }
}
