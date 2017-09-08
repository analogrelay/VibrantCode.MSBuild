using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace VibrantCode.MSBuild.Git
{
    public class GetGitInformation : Task
    {
        /// <summary>
        /// Specifies the path to the '.git' directory. Optional, inferred by searching from the current project up if not provided
        /// </summary>
        public string GitDirectory { get; set; }

        /// <summary>
        /// The path to the project from which a search for the '.git' directory should begin.
        /// </summary>
        public string ProjectDirectory { get; set; }

        /// <summary>
        /// Output parameter: Provides the Commit Hash
        /// </summary>
        public string CommitHash { get; set; }

        /// <summary>
        /// Output parameter: Provides the Branch Name
        /// </summary>
        public string BranchName { get; set; }

        public override bool Execute()
        {
            if(string.IsNullOrEmpty(GitDirectory))
            {
                if(string.IsNullOrEmpty(ProjectDirectory))
                {
                    Log.LogError("One of the ProjectDirectory or GitDirectory parameters must be provided to the GetGitInformation task.");
                    return false;
                }
                GitDirectory = LocateGitDirectory(ProjectDirectory);
            }

            if(!Directory.Exists(GitDirectory))
            {
                Log.LogMessage($"Unable to locate '{GitDirectory}'. This may not be a git repository");
                return true;
            }

            // Why not exec "git"? Well, that just seems unnecessary when it's pretty easy to do this.
            // If git ever moves these things around, we'll be number 5 million on the list of things broken by that change :).
            var headFile = Path.Combine(GitDirectory, "HEAD");
            if(!File.Exists(headFile))
            {
                Log.LogMessage($"Unable to locate head file '{headFile}'. This may not be a git repository");
                return true;
            }

            var headRef = File.ReadAllText(headFile);
            if(!headRef.StartsWith("ref: "))
            {
                // We're on a detached head
                Log.LogMessage("Current working directory is a detached head. Branch name will not be available");
                CommitHash = headRef;
                BranchName = null;
                return true;
            }
            headRef = headRef.Substring(5);

            // Get the branch name
            if(headRef.StartsWith("refs/heads/"))
            {
                BranchName = headRef.Substring(11);
            }
            else
            {
                Log.LogMessage("Head is not pointing to a branch, cannot provide the branch name.");
            }

            var refFile = Path.Combine(GitDirectory, headRef);
            if(!File.Exists(refFile))
            {
                Log.LogMessage($"Unable to locate ref file '{refFile}'. This may not be a git repository");
                return true;
            }

            CommitHash = File.ReadAllText(refFile);

            return true;
        }

        private string LocateGitDirectory(string root)
        {
            var current = root;
            var candidate = Path.Combine(current, ".git");
            while(!string.IsNullOrEmpty(current) && !Directory.Exists(candidate))
            {
                current = Path.GetDirectoryName(current);
                if (!string.IsNullOrEmpty(current))
                {
                    candidate = Path.Combine(current, ".git");
                }
            }

            return candidate;
        }
    }
}
