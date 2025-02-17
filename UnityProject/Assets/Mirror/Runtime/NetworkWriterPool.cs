using System;

namespace Mirror
{
    /// <summary>Pooled NetworkWriter, automatically returned to pool when using 'using'</summary>
    public sealed class PooledNetworkWriter : NetworkWriter, IDisposable
    {
        public void Dispose() => NetworkWriterPool.Recycle(this);
    }

    /// <summary>Pool of NetworkWriters to avoid allocations.</summary>
    public static class NetworkWriterPool
    {
        // reuse Pool<T>
        // we still wrap it in NetworkWriterPool.Get/Recycle so we can reset the
        // position before reusing.
        // this is also more consistent with NetworkReaderPool where we need to
        // assign the internal buffer before reusing.
        static readonly Pool<PooledNetworkWriter> Pool = new Pool<PooledNetworkWriter>(
            () => new PooledNetworkWriter(),
            // initial capacity to avoid allocations in the first few frames
            // 1000 * 1200 bytes = around 1 MB.
            1000
        );

        /// <summary>Get a writer from the pool. Creates new one if pool is empty.</summary>
        public static PooledNetworkWriter GetWriter()
        {
            // grab from pool & reset position
            //CUSTOM UNITYSTATION CODE// So it can be safely gotten, Without Thread funnies
            lock (Pool)
            {
	            PooledNetworkWriter writer = Pool.Take();
	            writer.Reset();
	            return writer;
            }
        }

        /// <summary>Return a writer to the pool.</summary>
        public static void Recycle(PooledNetworkWriter writer)
        {
	        //CUSTOM UNITYSTATION CODE// So it can be safely Added back, Without Thread funnies
	        lock (Pool)
	        {
		        Pool.Return(writer);
	        }
        }
    }
}
