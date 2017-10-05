using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SourceCode.Chasm.IO
{
    internal static class AsyncParallelUtil
    {
        #region For

        public static Task For(int fromInclusive, int toInclusive, Func<int, Task> body, int maxDop, CancellationToken cancellationToken)
        {
            if (toInclusive < fromInclusive) throw new ArgumentOutOfRangeException(nameof(toInclusive));
            if (maxDop < -1 || maxDop == 0) throw new ArgumentOutOfRangeException(nameof(maxDop));
            if (body == null) throw new ArgumentNullException(nameof(body));

            var options = new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = maxDop
            };

            var block = new ActionBlock<int>(body, options);
            {
                for (var i = fromInclusive; i <= toInclusive; i++)
                    block.Post(i);
            }
            block.Complete();

            var task = block.Completion;
            return task;
        }

        #endregion

        #region ForEach

        public static Task ForEach<TSource>(IEnumerable<TSource> source, Func<TSource, Task> body, int maxDop, CancellationToken cancellationToken)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (maxDop < -1 || maxDop == 0) throw new ArgumentOutOfRangeException(nameof(maxDop));
            if (body == null) throw new ArgumentNullException(nameof(body));

            var options = new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = maxDop
            };

            var block = new ActionBlock<TSource>(body, options);
            {
                foreach (var item in source)
                    block.Post(item);
            }
            block.Complete();

            var task = block.Completion;
            return task;
        }

        #endregion
    }
}
