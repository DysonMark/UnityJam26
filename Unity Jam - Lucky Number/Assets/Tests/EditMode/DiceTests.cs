using NUnit.Framework;
using UnityJam.LuckyNumber.Core;

namespace UnityJam.LuckyNumber.Tests
{
    public class DiceTests
    {
        [Test]
        public void RollClean_AlwaysInRange()
        {
            var rng = new System.Random(42);
            for (int i = 0; i < 10000; i++)
            {
                Assert.That(DiceMath.RollClean(rng), Is.InRange(1, 6));
            }
        }

        // Simulator: run by hand from the Test Runner, results go to the console.
        [Test, Explicit]
        public void Simulate()
        {
            // TODO - math, will enable us to rest thousand sof rolls at at time to tune the balance
            // Log reach-target %, broke %, average final money.
        }
    }
}
