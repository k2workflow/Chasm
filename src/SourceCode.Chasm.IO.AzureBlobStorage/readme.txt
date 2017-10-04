
Azure Blobs
-----------
After several iterations on Page & Block blobs, it looks like Append Blobs work best.

BlockBlobs would intermittently fail to write with "InvalidBlockList". 
A retry would succeed but reading the blob would sometimes prove to be corrupt.
It looks like parallel operations (esp. in this specific method) are known to be problematic in the client libs. 

PageBlobs have a size requirement of being N x 512 bytes, which is wasteful in $ and network.

AppendBlobs are relatively new but look like they are a good fit for immutable write-only.
The client-libs, however, call out their lack of support for multiple-writer scenarios. 
Only a couple of methods can work in this manner: http://stackoverflow.com/questions/32530126/azure-cloudappendblob-errors-with-concurrent-access
