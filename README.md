## Chasm

[![Build status](https://ci.appveyor.com/api/projects/status/993h43mvva8iqb7u/branch/master?svg=true)](https://ci.appveyor.com/project/jcdickinson/chasm/branch/master)
[![Code Coverage](https://codecov.io/gh/k2workflow/Chasm/coverage.svg)](https://codecov.io/gh/k2workflow/Chasm)
[![CodeFactor](https://www.codefactor.io/repository/github/jannesrsa/chasm/badge)](https://www.codefactor.io/repository/github/jannesrsa/chasm)

### CAS = Content Addressable Storage

[Wikipedia](https://en.wikipedia.org/wiki/Content-addressable_storage)
>"CAS, is a mechanism for storing information that can be retrieved based on its content, not its storage location. 
>It is typically used for high-speed storage and retrieval of fixed content"

[Git](https://en.wikipedia.org/wiki/Git) is a well-known example of a product that uses CAS.
However Chasm does not aim to be a Git replacement. For example, there is no concept of a working tree.
Rather, this is meant to be a general purpose CAS store for any document type.

### Requirements

* CAS: Self-versioned, immutable store. 
* Single-instance storage of data: If two authors create the exact same documents, their repos should not differ in Object data.
* Efficient network/disk operations: XML is bad, JSON is better, Protobuf/etc are best
* Simple: Avoid the need to reinvent graph semantics, etc

### Getting started

**Getting started with Git and GitHub**

 * People new to GitHub should consider using [GitHub for Windows](http://windows.github.com/).
 * If you decide not to use GHFW you will need to:
  1. [Set up Git and connect to GitHub](http://help.github.com/win-set-up-git/)
  2. [Fork the Chasm repository](http://help.github.com/fork-a-repo/)
 * Finally you should look into [git - the simple guide](http://rogerdudler.github.com/git-guide/)

**Rules for Our Git Repository**

 * We use ["A successful Git branching model"](http://nvie.com/posts/a-successful-git-branching-model/). What this means is that:
   * You need to branch off of the [develop branch](https://github.com/k2workflow/Chasm) when creating new features or non-critical bug fixes.
   * Each logical unit of work must come from a single and unique branch:
     * A logical unit of work could be a set of related bugs or a feature.
     * You should wait for us to accept the pull request (or you can cancel it) before committing to that branch again.
     
### License

Chasm is licensed under the MIT license, which can be found in license.txt.

**Additional Restrictions**

 * We only accept code that is compatible with the MIT license (essentially, MIT and Public Domain).
 * Copying copy-left (GPL-style) code is strictly forbidden.
