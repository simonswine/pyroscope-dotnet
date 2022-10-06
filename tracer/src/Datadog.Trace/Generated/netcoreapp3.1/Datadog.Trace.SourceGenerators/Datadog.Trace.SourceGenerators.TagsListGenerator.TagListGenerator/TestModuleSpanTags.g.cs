﻿// <auto-generated/>
#nullable enable

using Datadog.Trace.Processors;
using Datadog.Trace.Tagging;

namespace Datadog.Trace.Ci.Tagging
{
    partial class TestModuleSpanTags
    {
        // TypeBytes = System.Text.Encoding.UTF8.GetBytes("test.type");
        private static readonly byte[] TypeBytes = new byte[] { 116, 101, 115, 116, 46, 116, 121, 112, 101 };
        // ModuleBytes = System.Text.Encoding.UTF8.GetBytes("test.module");
        private static readonly byte[] ModuleBytes = new byte[] { 116, 101, 115, 116, 46, 109, 111, 100, 117, 108, 101 };
        // BundleBytes = System.Text.Encoding.UTF8.GetBytes("test.bundle");
        private static readonly byte[] BundleBytes = new byte[] { 116, 101, 115, 116, 46, 98, 117, 110, 100, 108, 101 };
        // FrameworkBytes = System.Text.Encoding.UTF8.GetBytes("test.framework");
        private static readonly byte[] FrameworkBytes = new byte[] { 116, 101, 115, 116, 46, 102, 114, 97, 109, 101, 119, 111, 114, 107 };
        // FrameworkVersionBytes = System.Text.Encoding.UTF8.GetBytes("test.framework_version");
        private static readonly byte[] FrameworkVersionBytes = new byte[] { 116, 101, 115, 116, 46, 102, 114, 97, 109, 101, 119, 111, 114, 107, 95, 118, 101, 114, 115, 105, 111, 110 };
        // CIProviderBytes = System.Text.Encoding.UTF8.GetBytes("ci.provider.name");
        private static readonly byte[] CIProviderBytes = new byte[] { 99, 105, 46, 112, 114, 111, 118, 105, 100, 101, 114, 46, 110, 97, 109, 101 };
        // CIPipelineIdBytes = System.Text.Encoding.UTF8.GetBytes("ci.pipeline.id");
        private static readonly byte[] CIPipelineIdBytes = new byte[] { 99, 105, 46, 112, 105, 112, 101, 108, 105, 110, 101, 46, 105, 100 };
        // CIPipelineNameBytes = System.Text.Encoding.UTF8.GetBytes("ci.pipeline.name");
        private static readonly byte[] CIPipelineNameBytes = new byte[] { 99, 105, 46, 112, 105, 112, 101, 108, 105, 110, 101, 46, 110, 97, 109, 101 };
        // CIPipelineNumberBytes = System.Text.Encoding.UTF8.GetBytes("ci.pipeline.number");
        private static readonly byte[] CIPipelineNumberBytes = new byte[] { 99, 105, 46, 112, 105, 112, 101, 108, 105, 110, 101, 46, 110, 117, 109, 98, 101, 114 };
        // CIPipelineUrlBytes = System.Text.Encoding.UTF8.GetBytes("ci.pipeline.url");
        private static readonly byte[] CIPipelineUrlBytes = new byte[] { 99, 105, 46, 112, 105, 112, 101, 108, 105, 110, 101, 46, 117, 114, 108 };
        // CIJobUrlBytes = System.Text.Encoding.UTF8.GetBytes("ci.job.url");
        private static readonly byte[] CIJobUrlBytes = new byte[] { 99, 105, 46, 106, 111, 98, 46, 117, 114, 108 };
        // CIJobNameBytes = System.Text.Encoding.UTF8.GetBytes("ci.job.name");
        private static readonly byte[] CIJobNameBytes = new byte[] { 99, 105, 46, 106, 111, 98, 46, 110, 97, 109, 101 };
        // StageNameBytes = System.Text.Encoding.UTF8.GetBytes("ci.stage.name");
        private static readonly byte[] StageNameBytes = new byte[] { 99, 105, 46, 115, 116, 97, 103, 101, 46, 110, 97, 109, 101 };
        // CIWorkspacePathBytes = System.Text.Encoding.UTF8.GetBytes("ci.workspace_path");
        private static readonly byte[] CIWorkspacePathBytes = new byte[] { 99, 105, 46, 119, 111, 114, 107, 115, 112, 97, 99, 101, 95, 112, 97, 116, 104 };
        // GitRepositoryBytes = System.Text.Encoding.UTF8.GetBytes("git.repository_url");
        private static readonly byte[] GitRepositoryBytes = new byte[] { 103, 105, 116, 46, 114, 101, 112, 111, 115, 105, 116, 111, 114, 121, 95, 117, 114, 108 };
        // GitCommitBytes = System.Text.Encoding.UTF8.GetBytes("git.commit.sha");
        private static readonly byte[] GitCommitBytes = new byte[] { 103, 105, 116, 46, 99, 111, 109, 109, 105, 116, 46, 115, 104, 97 };
        // GitBranchBytes = System.Text.Encoding.UTF8.GetBytes("git.branch");
        private static readonly byte[] GitBranchBytes = new byte[] { 103, 105, 116, 46, 98, 114, 97, 110, 99, 104 };
        // GitTagBytes = System.Text.Encoding.UTF8.GetBytes("git.tag");
        private static readonly byte[] GitTagBytes = new byte[] { 103, 105, 116, 46, 116, 97, 103 };
        // GitCommitAuthorNameBytes = System.Text.Encoding.UTF8.GetBytes("git.commit.author.name");
        private static readonly byte[] GitCommitAuthorNameBytes = new byte[] { 103, 105, 116, 46, 99, 111, 109, 109, 105, 116, 46, 97, 117, 116, 104, 111, 114, 46, 110, 97, 109, 101 };
        // GitCommitAuthorEmailBytes = System.Text.Encoding.UTF8.GetBytes("git.commit.author.email");
        private static readonly byte[] GitCommitAuthorEmailBytes = new byte[] { 103, 105, 116, 46, 99, 111, 109, 109, 105, 116, 46, 97, 117, 116, 104, 111, 114, 46, 101, 109, 97, 105, 108 };
        // GitCommitCommitterNameBytes = System.Text.Encoding.UTF8.GetBytes("git.commit.committer.name");
        private static readonly byte[] GitCommitCommitterNameBytes = new byte[] { 103, 105, 116, 46, 99, 111, 109, 109, 105, 116, 46, 99, 111, 109, 109, 105, 116, 116, 101, 114, 46, 110, 97, 109, 101 };
        // GitCommitCommitterEmailBytes = System.Text.Encoding.UTF8.GetBytes("git.commit.committer.email");
        private static readonly byte[] GitCommitCommitterEmailBytes = new byte[] { 103, 105, 116, 46, 99, 111, 109, 109, 105, 116, 46, 99, 111, 109, 109, 105, 116, 116, 101, 114, 46, 101, 109, 97, 105, 108 };
        // GitCommitMessageBytes = System.Text.Encoding.UTF8.GetBytes("git.commit.message");
        private static readonly byte[] GitCommitMessageBytes = new byte[] { 103, 105, 116, 46, 99, 111, 109, 109, 105, 116, 46, 109, 101, 115, 115, 97, 103, 101 };
        // BuildSourceRootBytes = System.Text.Encoding.UTF8.GetBytes("build.source_root");
        private static readonly byte[] BuildSourceRootBytes = new byte[] { 98, 117, 105, 108, 100, 46, 115, 111, 117, 114, 99, 101, 95, 114, 111, 111, 116 };
        // LibraryVersionBytes = System.Text.Encoding.UTF8.GetBytes("library_version");
        private static readonly byte[] LibraryVersionBytes = new byte[] { 108, 105, 98, 114, 97, 114, 121, 95, 118, 101, 114, 115, 105, 111, 110 };
        // RuntimeNameBytes = System.Text.Encoding.UTF8.GetBytes("runtime.name");
        private static readonly byte[] RuntimeNameBytes = new byte[] { 114, 117, 110, 116, 105, 109, 101, 46, 110, 97, 109, 101 };
        // RuntimeVersionBytes = System.Text.Encoding.UTF8.GetBytes("runtime.version");
        private static readonly byte[] RuntimeVersionBytes = new byte[] { 114, 117, 110, 116, 105, 109, 101, 46, 118, 101, 114, 115, 105, 111, 110 };
        // RuntimeArchitectureBytes = System.Text.Encoding.UTF8.GetBytes("runtime.architecture");
        private static readonly byte[] RuntimeArchitectureBytes = new byte[] { 114, 117, 110, 116, 105, 109, 101, 46, 97, 114, 99, 104, 105, 116, 101, 99, 116, 117, 114, 101 };
        // OSArchitectureBytes = System.Text.Encoding.UTF8.GetBytes("os.architecture");
        private static readonly byte[] OSArchitectureBytes = new byte[] { 111, 115, 46, 97, 114, 99, 104, 105, 116, 101, 99, 116, 117, 114, 101 };
        // OSPlatformBytes = System.Text.Encoding.UTF8.GetBytes("os.platform");
        private static readonly byte[] OSPlatformBytes = new byte[] { 111, 115, 46, 112, 108, 97, 116, 102, 111, 114, 109 };
        // OSVersionBytes = System.Text.Encoding.UTF8.GetBytes("os.version");
        private static readonly byte[] OSVersionBytes = new byte[] { 111, 115, 46, 118, 101, 114, 115, 105, 111, 110 };
        // GitCommitAuthorDateBytes = System.Text.Encoding.UTF8.GetBytes("git.commit.author.date");
        private static readonly byte[] GitCommitAuthorDateBytes = new byte[] { 103, 105, 116, 46, 99, 111, 109, 109, 105, 116, 46, 97, 117, 116, 104, 111, 114, 46, 100, 97, 116, 101 };
        // GitCommitCommitterDateBytes = System.Text.Encoding.UTF8.GetBytes("git.commit.committer.date");
        private static readonly byte[] GitCommitCommitterDateBytes = new byte[] { 103, 105, 116, 46, 99, 111, 109, 109, 105, 116, 46, 99, 111, 109, 109, 105, 116, 116, 101, 114, 46, 100, 97, 116, 101 };
        // CiEnvVarsBytes = System.Text.Encoding.UTF8.GetBytes("_dd.ci.env_vars");
        private static readonly byte[] CiEnvVarsBytes = new byte[] { 95, 100, 100, 46, 99, 105, 46, 101, 110, 118, 95, 118, 97, 114, 115 };
        // TestsSkippedBytes = System.Text.Encoding.UTF8.GetBytes("_dd.ci.itr.tests_skipped");
        private static readonly byte[] TestsSkippedBytes = new byte[] { 95, 100, 100, 46, 99, 105, 46, 105, 116, 114, 46, 116, 101, 115, 116, 115, 95, 115, 107, 105, 112, 112, 101, 100 };
        // StatusBytes = System.Text.Encoding.UTF8.GetBytes("test.status");
        private static readonly byte[] StatusBytes = new byte[] { 116, 101, 115, 116, 46, 115, 116, 97, 116, 117, 115 };

        public override string? GetTag(string key)
        {
            return key switch
            {
                "test.type" => Type,
                "test.module" => Module,
                "test.bundle" => Bundle,
                "test.framework" => Framework,
                "test.framework_version" => FrameworkVersion,
                "ci.provider.name" => CIProvider,
                "ci.pipeline.id" => CIPipelineId,
                "ci.pipeline.name" => CIPipelineName,
                "ci.pipeline.number" => CIPipelineNumber,
                "ci.pipeline.url" => CIPipelineUrl,
                "ci.job.url" => CIJobUrl,
                "ci.job.name" => CIJobName,
                "ci.stage.name" => StageName,
                "ci.workspace_path" => CIWorkspacePath,
                "git.repository_url" => GitRepository,
                "git.commit.sha" => GitCommit,
                "git.branch" => GitBranch,
                "git.tag" => GitTag,
                "git.commit.author.name" => GitCommitAuthorName,
                "git.commit.author.email" => GitCommitAuthorEmail,
                "git.commit.committer.name" => GitCommitCommitterName,
                "git.commit.committer.email" => GitCommitCommitterEmail,
                "git.commit.message" => GitCommitMessage,
                "build.source_root" => BuildSourceRoot,
                "library_version" => LibraryVersion,
                "runtime.name" => RuntimeName,
                "runtime.version" => RuntimeVersion,
                "runtime.architecture" => RuntimeArchitecture,
                "os.architecture" => OSArchitecture,
                "os.platform" => OSPlatform,
                "os.version" => OSVersion,
                "git.commit.author.date" => GitCommitAuthorDate,
                "git.commit.committer.date" => GitCommitCommitterDate,
                "_dd.ci.env_vars" => CiEnvVars,
                "_dd.ci.itr.tests_skipped" => TestsSkipped,
                "test.status" => Status,
                _ => base.GetTag(key),
            };
        }

        public override void SetTag(string key, string value)
        {
            switch(key)
            {
                case "test.type": 
                    Type = value;
                    break;
                case "test.module": 
                    Module = value;
                    break;
                case "test.framework": 
                    Framework = value;
                    break;
                case "test.framework_version": 
                    FrameworkVersion = value;
                    break;
                case "ci.provider.name": 
                    CIProvider = value;
                    break;
                case "ci.pipeline.id": 
                    CIPipelineId = value;
                    break;
                case "ci.pipeline.name": 
                    CIPipelineName = value;
                    break;
                case "ci.pipeline.number": 
                    CIPipelineNumber = value;
                    break;
                case "ci.pipeline.url": 
                    CIPipelineUrl = value;
                    break;
                case "ci.job.url": 
                    CIJobUrl = value;
                    break;
                case "ci.job.name": 
                    CIJobName = value;
                    break;
                case "ci.stage.name": 
                    StageName = value;
                    break;
                case "ci.workspace_path": 
                    CIWorkspacePath = value;
                    break;
                case "git.repository_url": 
                    GitRepository = value;
                    break;
                case "git.commit.sha": 
                    GitCommit = value;
                    break;
                case "git.branch": 
                    GitBranch = value;
                    break;
                case "git.tag": 
                    GitTag = value;
                    break;
                case "git.commit.author.name": 
                    GitCommitAuthorName = value;
                    break;
                case "git.commit.author.email": 
                    GitCommitAuthorEmail = value;
                    break;
                case "git.commit.committer.name": 
                    GitCommitCommitterName = value;
                    break;
                case "git.commit.committer.email": 
                    GitCommitCommitterEmail = value;
                    break;
                case "git.commit.message": 
                    GitCommitMessage = value;
                    break;
                case "build.source_root": 
                    BuildSourceRoot = value;
                    break;
                case "library_version": 
                    LibraryVersion = value;
                    break;
                case "runtime.name": 
                    RuntimeName = value;
                    break;
                case "runtime.version": 
                    RuntimeVersion = value;
                    break;
                case "runtime.architecture": 
                    RuntimeArchitecture = value;
                    break;
                case "os.architecture": 
                    OSArchitecture = value;
                    break;
                case "os.platform": 
                    OSPlatform = value;
                    break;
                case "os.version": 
                    OSVersion = value;
                    break;
                case "git.commit.author.date": 
                    GitCommitAuthorDate = value;
                    break;
                case "git.commit.committer.date": 
                    GitCommitCommitterDate = value;
                    break;
                case "_dd.ci.env_vars": 
                    CiEnvVars = value;
                    break;
                case "_dd.ci.itr.tests_skipped": 
                    TestsSkipped = value;
                    break;
                case "test.status": 
                    Status = value;
                    break;
                default: 
                    base.SetTag(key, value);
                    break;
            }
        }

        public override void EnumerateTags<TProcessor>(ref TProcessor processor)
        {
            if (Type is not null)
            {
                processor.Process(new TagItem<string>("test.type", Type, TypeBytes));
            }

            if (Module is not null)
            {
                processor.Process(new TagItem<string>("test.module", Module, ModuleBytes));
            }

            if (Bundle is not null)
            {
                processor.Process(new TagItem<string>("test.bundle", Bundle, BundleBytes));
            }

            if (Framework is not null)
            {
                processor.Process(new TagItem<string>("test.framework", Framework, FrameworkBytes));
            }

            if (FrameworkVersion is not null)
            {
                processor.Process(new TagItem<string>("test.framework_version", FrameworkVersion, FrameworkVersionBytes));
            }

            if (CIProvider is not null)
            {
                processor.Process(new TagItem<string>("ci.provider.name", CIProvider, CIProviderBytes));
            }

            if (CIPipelineId is not null)
            {
                processor.Process(new TagItem<string>("ci.pipeline.id", CIPipelineId, CIPipelineIdBytes));
            }

            if (CIPipelineName is not null)
            {
                processor.Process(new TagItem<string>("ci.pipeline.name", CIPipelineName, CIPipelineNameBytes));
            }

            if (CIPipelineNumber is not null)
            {
                processor.Process(new TagItem<string>("ci.pipeline.number", CIPipelineNumber, CIPipelineNumberBytes));
            }

            if (CIPipelineUrl is not null)
            {
                processor.Process(new TagItem<string>("ci.pipeline.url", CIPipelineUrl, CIPipelineUrlBytes));
            }

            if (CIJobUrl is not null)
            {
                processor.Process(new TagItem<string>("ci.job.url", CIJobUrl, CIJobUrlBytes));
            }

            if (CIJobName is not null)
            {
                processor.Process(new TagItem<string>("ci.job.name", CIJobName, CIJobNameBytes));
            }

            if (StageName is not null)
            {
                processor.Process(new TagItem<string>("ci.stage.name", StageName, StageNameBytes));
            }

            if (CIWorkspacePath is not null)
            {
                processor.Process(new TagItem<string>("ci.workspace_path", CIWorkspacePath, CIWorkspacePathBytes));
            }

            if (GitRepository is not null)
            {
                processor.Process(new TagItem<string>("git.repository_url", GitRepository, GitRepositoryBytes));
            }

            if (GitCommit is not null)
            {
                processor.Process(new TagItem<string>("git.commit.sha", GitCommit, GitCommitBytes));
            }

            if (GitBranch is not null)
            {
                processor.Process(new TagItem<string>("git.branch", GitBranch, GitBranchBytes));
            }

            if (GitTag is not null)
            {
                processor.Process(new TagItem<string>("git.tag", GitTag, GitTagBytes));
            }

            if (GitCommitAuthorName is not null)
            {
                processor.Process(new TagItem<string>("git.commit.author.name", GitCommitAuthorName, GitCommitAuthorNameBytes));
            }

            if (GitCommitAuthorEmail is not null)
            {
                processor.Process(new TagItem<string>("git.commit.author.email", GitCommitAuthorEmail, GitCommitAuthorEmailBytes));
            }

            if (GitCommitCommitterName is not null)
            {
                processor.Process(new TagItem<string>("git.commit.committer.name", GitCommitCommitterName, GitCommitCommitterNameBytes));
            }

            if (GitCommitCommitterEmail is not null)
            {
                processor.Process(new TagItem<string>("git.commit.committer.email", GitCommitCommitterEmail, GitCommitCommitterEmailBytes));
            }

            if (GitCommitMessage is not null)
            {
                processor.Process(new TagItem<string>("git.commit.message", GitCommitMessage, GitCommitMessageBytes));
            }

            if (BuildSourceRoot is not null)
            {
                processor.Process(new TagItem<string>("build.source_root", BuildSourceRoot, BuildSourceRootBytes));
            }

            if (LibraryVersion is not null)
            {
                processor.Process(new TagItem<string>("library_version", LibraryVersion, LibraryVersionBytes));
            }

            if (RuntimeName is not null)
            {
                processor.Process(new TagItem<string>("runtime.name", RuntimeName, RuntimeNameBytes));
            }

            if (RuntimeVersion is not null)
            {
                processor.Process(new TagItem<string>("runtime.version", RuntimeVersion, RuntimeVersionBytes));
            }

            if (RuntimeArchitecture is not null)
            {
                processor.Process(new TagItem<string>("runtime.architecture", RuntimeArchitecture, RuntimeArchitectureBytes));
            }

            if (OSArchitecture is not null)
            {
                processor.Process(new TagItem<string>("os.architecture", OSArchitecture, OSArchitectureBytes));
            }

            if (OSPlatform is not null)
            {
                processor.Process(new TagItem<string>("os.platform", OSPlatform, OSPlatformBytes));
            }

            if (OSVersion is not null)
            {
                processor.Process(new TagItem<string>("os.version", OSVersion, OSVersionBytes));
            }

            if (GitCommitAuthorDate is not null)
            {
                processor.Process(new TagItem<string>("git.commit.author.date", GitCommitAuthorDate, GitCommitAuthorDateBytes));
            }

            if (GitCommitCommitterDate is not null)
            {
                processor.Process(new TagItem<string>("git.commit.committer.date", GitCommitCommitterDate, GitCommitCommitterDateBytes));
            }

            if (CiEnvVars is not null)
            {
                processor.Process(new TagItem<string>("_dd.ci.env_vars", CiEnvVars, CiEnvVarsBytes));
            }

            if (TestsSkipped is not null)
            {
                processor.Process(new TagItem<string>("_dd.ci.itr.tests_skipped", TestsSkipped, TestsSkippedBytes));
            }

            if (Status is not null)
            {
                processor.Process(new TagItem<string>("test.status", Status, StatusBytes));
            }

            base.EnumerateTags(ref processor);
        }

        protected override void WriteAdditionalTags(System.Text.StringBuilder sb)
        {
            if (Type is not null)
            {
                sb.Append("test.type (tag):")
                  .Append(Type)
                  .Append(',');
            }

            if (Module is not null)
            {
                sb.Append("test.module (tag):")
                  .Append(Module)
                  .Append(',');
            }

            if (Bundle is not null)
            {
                sb.Append("test.bundle (tag):")
                  .Append(Bundle)
                  .Append(',');
            }

            if (Framework is not null)
            {
                sb.Append("test.framework (tag):")
                  .Append(Framework)
                  .Append(',');
            }

            if (FrameworkVersion is not null)
            {
                sb.Append("test.framework_version (tag):")
                  .Append(FrameworkVersion)
                  .Append(',');
            }

            if (CIProvider is not null)
            {
                sb.Append("ci.provider.name (tag):")
                  .Append(CIProvider)
                  .Append(',');
            }

            if (CIPipelineId is not null)
            {
                sb.Append("ci.pipeline.id (tag):")
                  .Append(CIPipelineId)
                  .Append(',');
            }

            if (CIPipelineName is not null)
            {
                sb.Append("ci.pipeline.name (tag):")
                  .Append(CIPipelineName)
                  .Append(',');
            }

            if (CIPipelineNumber is not null)
            {
                sb.Append("ci.pipeline.number (tag):")
                  .Append(CIPipelineNumber)
                  .Append(',');
            }

            if (CIPipelineUrl is not null)
            {
                sb.Append("ci.pipeline.url (tag):")
                  .Append(CIPipelineUrl)
                  .Append(',');
            }

            if (CIJobUrl is not null)
            {
                sb.Append("ci.job.url (tag):")
                  .Append(CIJobUrl)
                  .Append(',');
            }

            if (CIJobName is not null)
            {
                sb.Append("ci.job.name (tag):")
                  .Append(CIJobName)
                  .Append(',');
            }

            if (StageName is not null)
            {
                sb.Append("ci.stage.name (tag):")
                  .Append(StageName)
                  .Append(',');
            }

            if (CIWorkspacePath is not null)
            {
                sb.Append("ci.workspace_path (tag):")
                  .Append(CIWorkspacePath)
                  .Append(',');
            }

            if (GitRepository is not null)
            {
                sb.Append("git.repository_url (tag):")
                  .Append(GitRepository)
                  .Append(',');
            }

            if (GitCommit is not null)
            {
                sb.Append("git.commit.sha (tag):")
                  .Append(GitCommit)
                  .Append(',');
            }

            if (GitBranch is not null)
            {
                sb.Append("git.branch (tag):")
                  .Append(GitBranch)
                  .Append(',');
            }

            if (GitTag is not null)
            {
                sb.Append("git.tag (tag):")
                  .Append(GitTag)
                  .Append(',');
            }

            if (GitCommitAuthorName is not null)
            {
                sb.Append("git.commit.author.name (tag):")
                  .Append(GitCommitAuthorName)
                  .Append(',');
            }

            if (GitCommitAuthorEmail is not null)
            {
                sb.Append("git.commit.author.email (tag):")
                  .Append(GitCommitAuthorEmail)
                  .Append(',');
            }

            if (GitCommitCommitterName is not null)
            {
                sb.Append("git.commit.committer.name (tag):")
                  .Append(GitCommitCommitterName)
                  .Append(',');
            }

            if (GitCommitCommitterEmail is not null)
            {
                sb.Append("git.commit.committer.email (tag):")
                  .Append(GitCommitCommitterEmail)
                  .Append(',');
            }

            if (GitCommitMessage is not null)
            {
                sb.Append("git.commit.message (tag):")
                  .Append(GitCommitMessage)
                  .Append(',');
            }

            if (BuildSourceRoot is not null)
            {
                sb.Append("build.source_root (tag):")
                  .Append(BuildSourceRoot)
                  .Append(',');
            }

            if (LibraryVersion is not null)
            {
                sb.Append("library_version (tag):")
                  .Append(LibraryVersion)
                  .Append(',');
            }

            if (RuntimeName is not null)
            {
                sb.Append("runtime.name (tag):")
                  .Append(RuntimeName)
                  .Append(',');
            }

            if (RuntimeVersion is not null)
            {
                sb.Append("runtime.version (tag):")
                  .Append(RuntimeVersion)
                  .Append(',');
            }

            if (RuntimeArchitecture is not null)
            {
                sb.Append("runtime.architecture (tag):")
                  .Append(RuntimeArchitecture)
                  .Append(',');
            }

            if (OSArchitecture is not null)
            {
                sb.Append("os.architecture (tag):")
                  .Append(OSArchitecture)
                  .Append(',');
            }

            if (OSPlatform is not null)
            {
                sb.Append("os.platform (tag):")
                  .Append(OSPlatform)
                  .Append(',');
            }

            if (OSVersion is not null)
            {
                sb.Append("os.version (tag):")
                  .Append(OSVersion)
                  .Append(',');
            }

            if (GitCommitAuthorDate is not null)
            {
                sb.Append("git.commit.author.date (tag):")
                  .Append(GitCommitAuthorDate)
                  .Append(',');
            }

            if (GitCommitCommitterDate is not null)
            {
                sb.Append("git.commit.committer.date (tag):")
                  .Append(GitCommitCommitterDate)
                  .Append(',');
            }

            if (CiEnvVars is not null)
            {
                sb.Append("_dd.ci.env_vars (tag):")
                  .Append(CiEnvVars)
                  .Append(',');
            }

            if (TestsSkipped is not null)
            {
                sb.Append("_dd.ci.itr.tests_skipped (tag):")
                  .Append(TestsSkipped)
                  .Append(',');
            }

            if (Status is not null)
            {
                sb.Append("test.status (tag):")
                  .Append(Status)
                  .Append(',');
            }

            base.WriteAdditionalTags(sb);
        }
    }
}
