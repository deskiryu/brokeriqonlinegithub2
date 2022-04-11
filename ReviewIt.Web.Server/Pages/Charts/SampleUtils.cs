
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;

namespace ReviewIt.Web.Pages.Samples.Shared
{
    public static class SampleUtils
    {
        private static readonly Random _rng = new Random();

        public static class ChartColors
        {
            private static readonly Lazy<IReadOnlyList<Color>> _all = new Lazy<IReadOnlyList<Color>>(() => new Color[3]
            {
                BIQYellow, BIQDarkGray, BIQLightGray
            });

            public static IReadOnlyList<Color> All => _all.Value;

            public static readonly Color Red = Color.FromArgb(255, 99, 132);
            public static readonly Color Orange = Color.FromArgb(255, 159, 64);
            public static readonly Color Yellow = Color.FromArgb(255, 205, 86);
            public static readonly Color Green = Color.FromArgb(75, 192, 192);
            public static readonly Color Blue = Color.FromArgb(54, 162, 235);
            public static readonly Color Purple = Color.FromArgb(153, 102, 255);
            public static readonly Color Grey = Color.FromArgb(201, 203, 207);
            public static readonly Color Black = Color.FromArgb(0, 0, 0);

            public static readonly Color BIQYellow = Color.FromArgb(0xff, 0xfd, 0x7e);
            public static readonly Color BIQLightGray = Color.FromArgb(0x58, 0x58, 0x58);
            public static readonly Color BIQDarkGray = Color.FromArgb(0x3b, 0x3b, 0x3a);
        }

        public static IReadOnlyList<string> Months { get; } = new ReadOnlyCollection<string>(new[]
        {
            "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"
        });

        private static int RandomScalingFactorThreadUnsafe() => _rng.Next(-100, 100);

        public static int RandomScalingFactor()
        {
            lock (_rng)
            {
                return RandomScalingFactorThreadUnsafe();
            }
        }

        public static IEnumerable<int> RandomScalingFactor(int count)
        {
            int[] factors = new int[count];
            lock (_rng)
            {
                for (int i = 0; i < count; i++)
                {
                    factors[i] = RandomScalingFactorThreadUnsafe();
                }
            }

            return factors;
        }

        public static IEnumerable<DateTime> GetNextDays(int count)
        {
            DateTime now = DateTime.Now;
            DateTime[] factors = new DateTime[count];
            for (int i = 0; i < factors.Length; i++)
            {
                factors[i] = now.AddDays(i);
            }

            return factors;
        }
    }
}
