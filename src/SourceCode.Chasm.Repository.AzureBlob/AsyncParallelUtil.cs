using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SourceCode.Chasm.IO
{
    internal static class AsyncParallelUtil
    {
        #region For

        public static async Task ForAsync(int fromInclusive, int toInclusive, ParallelOptions options, Func<int, Task> body)
        {
            if (toInclusive < fromInclusive) throw new ArgumentOutOfRangeException(nameof(toInclusive));
            if (body == null) throw new ArgumentNullException(nameof(body));

            var opt = new ExecutionDataflowBlockOptions
            {
                CancellationToken = options == null ? CancellationToken.None : options.CancellationToken,
                MaxDegreeOfParallelism = options == null ? -1 : options.MaxDegreeOfParallelism
            };

            var block = new ActionBlock<int>(body, opt);
            {
                // Send
                for (var i = fromInclusive; i <= toInclusive; i++)
                    await block.SendAsync(i, opt.CancellationToken).ConfigureAwait(false);
            }
            block.Complete();
            await block.Completion;
        }

        public static async ValueTask<IReadOnlyDictionary<int, TValue>> ForAsync<TValue>(int fromInclusive, int toInclusive, ParallelOptions options, Func<int, Task<TValue>> body)
        {
            if (toInclusive < fromInclusive) throw new ArgumentOutOfRangeException(nameof(toInclusive));
            if (body == null) throw new ArgumentNullException(nameof(body));

            var opt = new ExecutionDataflowBlockOptions
            {
                CancellationToken = options == null ? CancellationToken.None : options.CancellationToken,
                MaxDegreeOfParallelism = options == null ? -1 : options.MaxDegreeOfParallelism
            };

            var list = new ConcurrentDictionary<int, TValue>();

            var block = new TransformBlock<int, TValue>(body, opt);
            {
                // Send
                for (var i = fromInclusive; i <= toInclusive; i++)
                    await block.SendAsync(i, opt.CancellationToken).ConfigureAwait(false);

                // Receive
                for (var i = fromInclusive; i <= toInclusive; i++)
                    list[i] = await block.ReceiveAsync(opt.CancellationToken).ConfigureAwait(false);
            }
            block.Complete();
            await block.Completion;

            return list;
        }

        #endregion

        #region ForEach

        public static async Task ForEachAsync<TSource>(IEnumerable<TSource> source, ParallelOptions options, Func<TSource, Task> body)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (body == null) throw new ArgumentNullException(nameof(body));

            var opt = new ExecutionDataflowBlockOptions
            {
                CancellationToken = options == null ? CancellationToken.None : options.CancellationToken,
                MaxDegreeOfParallelism = options == null ? -1 : options.MaxDegreeOfParallelism
            };

            var block = new ActionBlock<TSource>(body, opt);
            {
                // Send
                foreach (var item in source)
                    await block.SendAsync(item, opt.CancellationToken).ConfigureAwait(false);
            }
            block.Complete();
            await block.Completion;
        }

        public static async ValueTask<IReadOnlyDictionary<TSource, TValue>> ForEachAsync<TSource, TValue>(IEnumerable<TSource> source, ParallelOptions options, Func<TSource, Task<KeyValuePair<TSource, TValue>>> body)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (body == null) throw new ArgumentNullException(nameof(body));

            var opt = new ExecutionDataflowBlockOptions
            {
                CancellationToken = options == null ? CancellationToken.None : options.CancellationToken,
                MaxDegreeOfParallelism = options == null ? -1 : options.MaxDegreeOfParallelism
            };

            var list = new ConcurrentDictionary<TSource, TValue>();

            var block = new TransformBlock<TSource, KeyValuePair<TSource, TValue>>(body, opt);
            {
                // Send
                foreach (var item in source)
                {
                    await block.SendAsync(item, opt.CancellationToken).ConfigureAwait(false);
                }

                // Receive
                for (var i = 0; i <= block.OutputCount; i++)
                {
                    var item = await block.ReceiveAsync(opt.CancellationToken).ConfigureAwait(false);
                    list[item.Key] = item.Value;
                }
            }
            block.Complete();
            await block.Completion;

            return list;
        }

        #endregion
    }
}
