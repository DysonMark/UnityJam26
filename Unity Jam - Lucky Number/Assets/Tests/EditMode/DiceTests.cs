using System;
using NUnit.Framework;
using UnityJam.LuckyNumber.Core;

namespace UnityJam.LuckyNumber.Tests
{
    public class DiceTests
    {
        private const int Rolls = 60000;
        private const double Tolerance = 0.01;

        // Fraction of rolls that landed on each face. base 1 array, so index 0 is unused, faces[3] means a 3.
        private static double[] Frequencies(Func<int> roll)
        {
            var faces = new double[DiceMath.Faces + 1];
            for (int i = 0; i < Rolls; i++) faces[roll()]++;
            for (int n = 1; n <= DiceMath.Faces; n++) faces[n] /= Rolls;
            return faces;
        }

        [Test]
        public void RollClean_AlwaysInRange()
        {
            var rng = new Random(42);
            for (int i = 0; i < 10000; i++)
            {
                Assert.That(DiceMath.RollClean(rng), Is.InRange(1, 6));
            }
        }

        [Test]
        public void RollClean_FacesAreEven()
        {
            var rng = new Random(42);
            var faces = Frequencies(() => DiceMath.RollClean(rng));
            for (int n = 1; n <= DiceMath.Faces; n++)
            {
                Assert.That(faces[n], Is.EqualTo(1.0 / 6).Within(Tolerance), $"face {n}");
            }
        }

        [Test]
        public void RollLoaded_HitsLuckyAtLoadedChance_OtherFacesEven()
        {
            var rng = new Random(42);
            const int lucky = 4;
            const float chance = 0.5f;
            var faces = Frequencies(() => DiceMath.RollLoaded(rng, lucky, chance));

            Assert.That(faces[lucky], Is.EqualTo(chance).Within(Tolerance), "lucky face");
            for (int n = 1; n <= DiceMath.Faces; n++)
            {
                if (n == lucky) continue;
                Assert.That(faces[n], Is.EqualTo((1 - chance) / 5).Within(Tolerance), $"face {n}");
            }
        }

        [Test]
        public void RollLoaded_ZeroChance_NeverLucky()
        {
            var rng = new Random(42);
            for (int i = 0; i < 10000; i++)
            {
                int roll = DiceMath.RollLoaded(rng, 6, 0f);
                Assert.That(roll, Is.InRange(1, 5));
            }
        }

        [Test]
        public void RollLoaded_FullChance_AlwaysLucky()
        {
            // TODO - a fully loaded die should always land on the lucky number.
            // Delete the Assert.Ignore line when done.
            Assert.Ignore("TODO - Dyse");
        }

        [Test]
        public void SameSeed_SameSequence()
        {
            // TODO - the same seed should always give the same rolls.
            // Bonus: make it fail on purpose to prove it can catch a bug.
            // Delete the Assert.Ignore line when done.
            Assert.Ignore("TODO - Dyse");
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
