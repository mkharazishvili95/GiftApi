using FluentValidation.TestHelper;
using GiftApi.Application.Features.User.Commands.Password.Change;

namespace GiftApi.Tests.User
{
    [TestFixture]
    public class ChangePasswordValidationTests
    {
        private ChangePasswordValidation _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new ChangePasswordValidation();
        }

        [Test]
        public void Should_HaveError_When_OldPassword_IsEmpty()
        {
            var command = new ChangePasswordCommand { OldPassword = "" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.OldPassword)
                  .WithErrorMessage("Old password is required");
        }

        [Test]
        public void Should_HaveError_When_NewPassword_IsEmpty()
        {
            var command = new ChangePasswordCommand { NewPassword = "" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.NewPassword)
                  .WithErrorMessage("New password is required");
        }

        [Test]
        public void Should_HaveError_When_ConfirmPassword_IsEmpty()
        {
            var command = new ChangePasswordCommand { ConfirmPassword = "" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
                  .WithErrorMessage("Confirm password is required");
        }

        [Test]
        public void Should_HaveError_When_NewPassword_IsTooShort()
        {
            var command = new ChangePasswordCommand { OldPassword = "OldPass1", NewPassword = "123", ConfirmPassword = "123" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.NewPassword)
                  .WithErrorMessage("New password must be at least 6 characters");
        }

        [Test]
        public void Should_HaveError_When_NewPassword_Same_As_OldPassword()
        {
            var command = new ChangePasswordCommand { OldPassword = "SamePass1", NewPassword = "SamePass1", ConfirmPassword = "SamePass1" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.NewPassword)
                  .WithErrorMessage("New password must be different from old password");
        }

        [Test]
        public void Should_HaveError_When_ConfirmPassword_DoesNotMatch()
        {
            var command = new ChangePasswordCommand { OldPassword = "OldPass1", NewPassword = "NewPass1", ConfirmPassword = "Mismatch1" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
                  .WithErrorMessage("Passwords do not match");
        }

        [Test]
        public void Should_NotHaveError_When_Command_IsValid()
        {
            var command = new ChangePasswordCommand { OldPassword = "OldPass1", NewPassword = "NewPass123", ConfirmPassword = "NewPass123" };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}