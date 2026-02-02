using GuestFlow.Application.Operations.AI;
using Xunit;

namespace GuestFlow.Application.Tests.Operations.AI
{
    public class PIIMaskingServiceTests
    {
        private readonly PIIMaskingService _service;

        public PIIMaskingServiceTests()
        {
            _service = new PIIMaskingService();
        }

        [Fact]
        public void MaskPII_ShouldMaskEmail()
        {
            var input = "Bana test@example.com üzerinden ulaşabilirsiniz.";
            var expected = "Bana [EMAIL_MASKED] üzerinden ulaşabilirsiniz.";
            var result = _service.MaskPII(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MaskPII_ShouldMaskPhone()
        {
            var input = "Telefon numaram +905554443322.";
            var result = _service.MaskPII(input);
            Assert.Contains("[PHONE_MASKED]", result);
        }

        [Fact]
        public void MaskPII_ShouldMaskMultiple()
        {
            var input = "Email: john@doe.com, Tel: 05321112233";
            var result = _service.MaskPII(input);
            Assert.Contains("[EMAIL_MASKED]", result);
            Assert.Contains("[PHONE_MASKED]", result);
        }
    }
}
