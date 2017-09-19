Introduction
------------
CAS = Content Addressable Storage

https://en.wikipedia.org/wiki/Content-addressable_storage
"CAS, is a mechanism for storing information that can be retrieved based on its content, not its storage location. 
It is typically used for high-speed storage and retrieval of fixed content"

Git is a well-known example of a product that uses CAS.


Requirements
------------
* CAS: Self-versioned, immutable store. So that versioning is intrinsic, deployment (P&D) is self-describing and functional benefits of immutable code is realized.
* Single-instance storage of data: If you and I both describe the same Northwind db, our 'repos' should differ only in Ref data, not Object data.
* Efficient network/disk operations: XML is bad, JSON is better, Bond/Protobuf/etc are best
* Simple: Avoid the need to reinvent graph semantics, etc 


Existing tech
-------------
So why not just use Git/VSTS/etc
The problem is that we don't need a working tree, and in fact a working tree will make concurrent work challenging.
For example, you and I describe Northwind at the same time on same worker. Implies two working trees, two commits and thus possible conflicts.
Conflict resolution in a headless environment is challenging.

What we need is a single, shared, ODB (back end store for git objects) so that we can optimize CRUD and caching.
The latest git client has just started supporting multiple working trees, but it's early code and it would also result in duplicate data locally.
Git also has some backend support for 'remote branches' but the client support is mostly missing.


Azure Blob
----------
After several iterations on Page & Block blobs, it looks like we need to use Append Blobs.

BlockBlobs would intermittently fail to write with "InvalidBlockList". 
A retry would succeed but reading the blob would sometimes prove to be corrupt.
It looks like parallel operations (esp. in this specific method) are known to be problematic in the client libs. 

PageBlobs have a size requirement of being N x 512 bytes, which is wasteful in $ and network.

AppendBlobs are new but look like they are a good fit for immutable write-only.
The client-libs, however, call out their lack of support for multiple-writer scenarios. 
Only a couple of methods can work in this manner: http://stackoverflow.com/questions/32530126/azure-cloudappendblob-errors-with-concurrent-access


Disk Cache
----------
Obviously, fetching each item across the wire would be expensive, so a Disk Cache is used to mitigate the IO.
The cache uses a simple MRU mechanism to scavenge any files older than a specified timespan.
Note that the CAS design guarantees that (bugs withstanding) cache items are never stale - if present they are 'correct'


Naming
------
Why is CommitRef not called Branch? 
Because it is just one kind of Ref. Git happens to call this kind of Ref a 'branch', but that's not its only use.
In other words, that can be considered an implementation detail of Git.

