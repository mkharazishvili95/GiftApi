//using FluentValidation.TestHelper;
//using GiftApi.Application.Features.Voucher.Commands.Buy;
//using NUnit.Framework;
//using System;

//namespace GiftApi.Tests.Voucher
//{
//    [TestFixture]
//    public class BuyVoucherValidationTests
//    {
//        private BuyVoucherValidation _validator;

//        [SetUp]
//        public void Setup()
//        {
//            _validator = new BuyVoucherValidation();
//        }

//        [Test]
//        public void Should_HaveError_When_VoucherIdIsEmpty()
//        {
//            var command = new BuyVoucherCommand { VoucherId = Guid.Empty };
//            var result = _validator.TestValidate(command);
//            result.ShouldHaveValidationErrorFor(x => x.VoucherId)
//                  .WithErrorMessage("VoucherId is required.");
//        }

//        [Test]
//        public void Should_HaveError_When_UserIdIsEmpty()
//        {
//            var command = new BuyVoucherCommand { UserId = Guid.Empty };
//            var result = _validator.TestValidate(command);
//            result.ShouldHaveValidationErrorFor(x => x.UserId)
//                  .WithErrorMessage("UserId is required.");
//        }

//        [Test]
//        public void Should_HaveError_When_AmountIsZeroOrNegative()
//        {
//            var command = new BuyVoucherCommand { Amount = 0 };
//            var result = _validator.TestValidate(command);
//            result.ShouldHaveValidationErrorFor(x => x.Amount)
//                  .WithErrorMessage("Amount must be greater than zero.");
//        }

//        [Test]
//        public void Should_HaveError_When_QuantityIsZeroOrNegative()
//        {
//            var command = new BuyVoucherCommand { Quantity = 0 };
//            var result = _validator.TestValidate(command);
//            result.ShouldHaveValidationErrorFor(x => x.Quantity)
//                  .WithErrorMessage("Quantity must be greater than zero.");
//        }

//        [Test]
//        public void Should_HaveError_When_RecipientNameIsEmptyOrTooLong()
//        {
//            var command1 = new BuyVoucherCommand { RecipientName = "" };
//            var command2 = new BuyVoucherCommand { RecipientName = new string('a', 101) };

//            _validator.TestValidate(command1)
//                .ShouldHaveValidationErrorFor(x => x.RecipientName)
//                .WithErrorMessage("Recipient name is required.");

//            _validator.TestValidate(command2)
//                .ShouldHaveValidationErrorFor(x => x.RecipientName)
//                .WithErrorMessage("Recipient name must not exceed 100 characters.");
//        }

//        [Test]
//        public void Should_HaveError_When_RecipientCityIsEmptyOrTooLong()
//        {
//            var command1 = new BuyVoucherCommand { RecipientCity = "" };
//            var command2 = new BuyVoucherCommand { RecipientCity = new string('a', 101) };

//            _validator.TestValidate(command1)
//                .ShouldHaveValidationErrorFor(x => x.RecipientCity)
//                .WithErrorMessage("Recipient city is required.");

//            _validator.TestValidate(command2)
//                .ShouldHaveValidationErrorFor(x => x.RecipientCity)
//                .WithErrorMessage("Recipient city must not exceed 100 characters.");
//        }

//        [Test]
//        public void Should_HaveError_When_RecipientAddressIsEmptyOrTooLong()
//        {
//            var command1 = new BuyVoucherCommand { RecipientAddress = "" };
//            var command2 = new BuyVoucherCommand { RecipientAddress = new string('a', 201) };

//            _validator.TestValidate(command1)
//                .ShouldHaveValidationErrorFor(x => x.RecipientAddress)
//                .WithErrorMessage("Recipient address is required.");

//            _validator.TestValidate(command2)
//                .ShouldHaveValidationErrorFor(x => x.RecipientAddress)
//                .WithErrorMessage("Recipient address must not exceed 200 characters.");
//        }

//        [Test]
//        public void Should_HaveError_When_RecipientPhoneIsEmptyOrInvalid()
//        {
//            var command1 = new BuyVoucherCommand { RecipientPhone = "" };
//            var command2 = new BuyVoucherCommand { RecipientPhone = "123456789" };
//            var command3 = new BuyVoucherCommand { RecipientPhone = "59812345" }; // too short

//            _validator.TestValidate(command1)
//                .ShouldHaveValidationErrorFor(x => x.RecipientPhone)
//                .WithErrorMessage("Recipient phone is required.");

//            _validator.TestValidate(command2)
//                .ShouldHaveValidationErrorFor(x => x.RecipientPhone)
//                .WithErrorMessage("Recipient phone must be a valid Georgian mobile number starting with '5' (e.g., 598123456).");

//            _validator.TestValidate(command3)
//                .ShouldHaveValidationErrorFor(x => x.RecipientPhone)
//                .WithErrorMessage("Recipient phone must be a valid Georgian mobile number starting with '5' (e.g., 598123456).");
//        }

//        [Test]
//        public void Should_NotHaveError_When_CommandIsValid()
//        {
//            var command = new BuyVoucherCommand
//            {
//                VoucherId = Guid.NewGuid(),
//                UserId = Guid.NewGuid(),
//                Amount = 50,
//                Quantity = 1,
//                RecipientName = "John Doe",
//                RecipientCity = "Tbilisi",
//                RecipientAddress = "Street 1",
//                RecipientPhone = "598123456"
//            };

//            var result = _validator.TestValidate(command);
//            result.ShouldNotHaveAnyValidationErrors();
//        }
//    }
//}
