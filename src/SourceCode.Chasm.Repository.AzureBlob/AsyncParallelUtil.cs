using SourceCode.Clay.Collections.Generic;
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

        public static async Task ForAsync(int from, int to, ParallelOptions options, Func<int, Task> body)
        {
            if (to < from) throw new ArgumentOutOfRangeException(nameof(to));
            if (body == null) throw new ArgumentNullException(nameof(body));

            var opt = new ExecutionDataflowBlockOptions
            {
                CancellationToken = options == null ? CancellationToken.None : options.CancellationToken,
                MaxDegreeOfParallelism = options == null ? -1 : options.MaxDegreeOfParallelism
            };

            var block = new ActionBlock<int>(body, opt);

            // Send
            for (var i = from; i < to; i++)
            {
                await block.SendAsync(i, opt.CancellationToken).ConfigureAwait(false);
            }

            block.Complete();
            await block.Completion;
        }

        public static async ValueTask<IReadOnlyDictionary<int, TValue>> ForAsync<TValue>(int from, int to, ParallelOptions options, Func<int, Task<TValue>> body)
        {
            if (to < from) throw new ArgumentOutOfRangeException(nameof(to));
            if (body == null) throw new ArgumentNullException(nameof(body));

            var dict = new ConcurrentDictionary<int, TValue>();

            var opt = new ExecutionDataflowBlockOptions
            {
                CancellationToken = options == null ? CancellationToken.None : options.CancellationToken,
                MaxDegreeOfParallelism = options == null ? -1 : options.MaxDegreeOfParallelism
            };

            var block = new TransformBlock<int, TValue>(body, opt);

            // Send
            for (var i = from; i < to; i++)
            {
                await block.SendAsync(i, opt.CancellationToken).ConfigureAwait(false);
            }

            // Receive
            for (var i = from; i < to; i++)
            {
                var value = await block.ReceiveAsync(opt.CancellationToken).ConfigureAwait(false);
                dict[i] = value;
            }

            block.Complete();
            await block.Completion;

            return dict;
        }

        #endregion

        #region ForEach

        public static async Task ForEachAsync<TSource>(IEnumerable<TSource> source, ParallelOptions options, Func<TSource, Task> body)
        {
            if (source == null) return;
            if (body == null) throw new ArgumentNullException(nameof(body));

            var opt = new ExecutionDataflowBlockOptions
            {
                CancellationToken = options == null ? CancellationToken.None : options.CancellationToken,
                MaxDegreeOfParallelism = options == null ? -1 : options.MaxDegreeOfParallelism
            };

            var block = new ActionBlock<TSource>(body, opt);

            // Send
            foreach (var item in source)
            {
                await block.SendAsync(item, opt.CancellationToken).ConfigureAwait(false);
            }

            block.Complete();
            await block.Completion;
        }

        public static async ValueTask<IReadOnlyDictionary<TSource, TValue>> ForEachAsync<TSource, TValue>(IEnumerable<TSource> source, ParallelOptions options, Func<TSource, Task<KeyValuePair<TSource, TValue>>> body)
        {
            if (source == null) return ReadOnlyDictionary.Empty<TSource, TValue>();
            if (body == null) throw new ArgumentNullException(nameof(body));

            var dict = new ConcurrentDictionary<TSource, TValue>();

            var opt = new ExecutionDataflowBlockOptions
            {
                CancellationToken = options == null ? CancellationToken.None : options.CancellationToken,
                MaxDegreeOfParallelism = options == null ? -1 : options.MaxDegreeOfParallelism
            };

            var block = new TransformBlock<TSource, KeyValuePair<TSource, TValue>>(body, opt);

            // Send
            foreach (var item in source)
            {
                await block.SendAsync(item, opt.CancellationToken).ConfigureAwait(false);
            }

            // Receive
            for (var i = 0; i <= block.OutputCount; i++)
            {
                var value = await block.ReceiveAsync(opt.CancellationToken).ConfigureAwait(false);
                dict[value.Key] = value.Value;
            }

            block.Complete();
            await block.Completion;

            return dict;
        }

        #endregion
    }
}
