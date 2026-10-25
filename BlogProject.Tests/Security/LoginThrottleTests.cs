using BlogProject.Business.Security;

namespace BlogProject.Tests.Security
{
    public class LoginThrottleTests
    {
        private sealed class ManualTime : TimeProvider
        {
            public DateTimeOffset Now { get; set; } = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            public override DateTimeOffset GetUtcNow() => Now;
        }

        [Fact]
        public void LocksAfterMaxFailures_AndUnlocksAfterDuration()
        {
            var time = new ManualTime();
            var throttle = new LoginThrottle(3, TimeSpan.FromMinutes(10), time);
            var key = LoginThrottle.Key("A@a.com", "1.2.3.4");

            throttle.RegisterFailure(key);
            throttle.RegisterFailure(key);
            Assert.False(throttle.IsLocked(key));
            throttle.RegisterFailure(key);
            Assert.True(throttle.IsLocked(key));

            time.Now = time.Now.AddMinutes(11);
            Assert.False(throttle.IsLocked(key));
        }

        [Fact]
        public void SuccessResetsCounter_AndKeysAreIndependent()
        {
            var throttle = new LoginThrottle(2, TimeSpan.FromMinutes(10), new ManualTime());
            var a = LoginThrottle.Key("a@a.com", "1.1.1.1");
            var b = LoginThrottle.Key("b@a.com", "1.1.1.1");

            throttle.RegisterFailure(a);
            throttle.Reset(a);
            throttle.RegisterFailure(a);
            Assert.False(throttle.IsLocked(a));

            throttle.RegisterFailure(a);
            Assert.True(throttle.IsLocked(a));
            Assert.False(throttle.IsLocked(b));
        }

        [Fact]
        public void Key_IgnoresCaseAndWhitespaceOfEmail()
        {
            Assert.Equal(LoginThrottle.Key("A@a.com ", "ip"), LoginThrottle.Key("a@A.com", "ip"));
        }
    }
}
