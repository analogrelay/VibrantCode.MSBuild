# VibrantCode.MSBuild.Git

Loads Git metadata into MSBuild properties.

## How do I use it?

Just reference the package! The MSBuild goodness within will make the following MSBuild properties available **automagically**

* `Git_CommitHash` - The full hash of the current commit on which the build is being run
* `Git_BranchName` - The name of the branch on which the build is being run (if any)

If you are on a detached HEAD (i.e. not on a branch), the `Git_BranchName` value will be empty, but `Git_CommitHash` will be set correctly. If you're not in a Git repo, nothing will fail, but both properties will be empty.

The properties are initialized in a target that's inserted **before** the `BeforeBuild` target. If you really want to make sure your targets run AFTER the Git metadata is loaded, use `AfterTargets` and set your targets to run after `GetGitInformation`.

If you want to control when the Git metadata is loaded, set the `AutomaticGetGitInformation` MSBuild property to `false`, that will disable the MSBuild target that loads the information. You can still use the MSBuild task though, like this:

```xml
<Target Name="GetGitInformation">
    <GetGitInformation ProjectDirectory="$(MSBuildProjectDirectory)">
        <Output TaskParameter="CommitHash" PropertyName="Git_CommitHash" />
        <Output TaskParameter="BranchName" PropertyName="Git_BranchName" />
    </GetGitInformation>
    <Message Text="Git Commit Info: CommitHash = $(Git_CommitHash), BranchName = $(Git_BranchName)" />
</Target>
```

## Can this help me with versioning?

Nope. This just puts Git metadata into MSBuild. What you do with it is up to you. Put it in a semantic version, slap it in some attributes. It's all cool.
