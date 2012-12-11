using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.TextToSchedule.Grammars
{
    /// <summary>
    /// Provides expression to a <see cref="ITextToSchedule"/> instance.
    /// </summary>
    public interface IGrammar
    {
        /// <summary>
        /// every [n] (sec|min|hour) [on mon-fri] [of monthspec] [time]
        /// </summary>
        string Expression1 { get; }


        /// <summary>
        /// (every|ordinal) (day of week) [of (month)] [time]
        /// </summary>
        string Expression2 { get; }

        /// <summary>
        /// [every|on] (date) [time]
        /// </summary>
        string Expression3 { get; }

        /// <summary>
        /// every [n] (days|weeks|months|years) (from [date]) (at [time])
        /// </summary>
        string Expression4 { get; }

        /// <summary>
        /// every [n] day of (month) [time]
        /// </summary>
        string Expression5 { get; }
    }
}
