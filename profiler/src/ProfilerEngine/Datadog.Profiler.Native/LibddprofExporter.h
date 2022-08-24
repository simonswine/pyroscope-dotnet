// Unless explicitly stated otherwise all files in this repository are licensed under the Apache 2 License.
// This product includes software developed at Datadog (https://www.datadoghq.com/). Copyright 2022 Datadog, Inc.

#pragma once
#include "IConfiguration.h"
#include "IExporter.h"
#include "TagsHelper.h"

#include <mutex>

extern "C"
{
#include "ddprof/ffi.h"
}

#include <forward_list>
#include <memory>
#include <string_view>
#include <unordered_map>
#include <vector>

class Sample;
class IMetricsSender;
class IApplicationStore;
class IRuntimeInfo;
class IEnabledProfilers;

class LibddprofExporter : public IExporter
{
public:
    LibddprofExporter(
        IConfiguration* configuration,
        IApplicationStore* applicationStore,
        IRuntimeInfo* runtimeInfo,
        IEnabledProfilers* enabledProfilers);
    ~LibddprofExporter() override;
    bool Export() override;
    void Add(Sample const& sample) override;

private:
    class SerializedProfile
    {
    public:
        SerializedProfile(struct ddprof_ffi_Profile* profile);
        ~SerializedProfile();

        ddprof_ffi_Vec_u8 GetBuffer() const;
        ddprof_ffi_Timespec GetStart() const;
        ddprof_ffi_Timespec GetEnd() const;

        bool IsValid() const;

    private:
        ddprof_ffi_SerializeResult _encodedProfile;
    };

    class Tags
    {
    public:
        Tags();
        ~Tags() noexcept;

        Tags(const Tags&) = delete;
        Tags& operator=(const Tags&) = delete;

        Tags(Tags&&) noexcept;
        Tags& operator=(Tags&&) noexcept;

        void Add(std::string const& name, std::string const& value);

        const ddprof_ffi_Vec_tag* GetFfiTags() const;

    private:
        ddprof_ffi_Vec_tag _ffiTags;
    };

    class ProfileAutoDelete
    {
    public:
        ProfileAutoDelete(struct ddprof_ffi_Profile* profile);
        ~ProfileAutoDelete();

    private:
        struct ddprof_ffi_Profile* _profile;
    };
        
    class ProfileInfo
    {
    public:
        ProfileInfo();
    public:
        ddprof_ffi_Profile* profile;
        std::int32_t samplesCount;
        std::int32_t exportsCount;
        std::mutex lock;
    };

    class ProfileInfoScope
    {
    public:
        ProfileInfoScope(ProfileInfo& profileInfo);

        ProfileInfo& profileInfo;

    private:
        std::lock_guard<std::mutex> _lockGuard;
    };

    static Tags CreateTags(
        IConfiguration* configuration,
        IRuntimeInfo* runtimeInfo,
        IEnabledProfilers* enabledProfilers);

    static ddprof_ffi_ProfileExporterV3* CreateExporter(const ddprof_ffi_Vec_tag* tags, ddprof_ffi_EndpointV3 endpoint);
    static ddprof_ffi_Profile* CreateProfile();

    ddprof_ffi_Request* CreateRequest(SerializedProfile const& encodedProfile, ddprof_ffi_ProfileExporterV3* exporter,  const Tags& additionalTags) const;
    ddprof_ffi_EndpointV3 CreateEndpoint(IConfiguration* configuration);
    ProfileInfoScope GetInfo(std::string_view runtimeId);

    void ExportToDisk(const std::string& applicationName, SerializedProfile const& encodedProfile, int idx);

    bool Send(ddprof_ffi_Request* request, ddprof_ffi_ProfileExporterV3* exporter) const;
    std::string GeneratePprofFilePath(const std::string& applicationName, int idx) const;
    fs::path CreatePprofOutputPath(IConfiguration* configuration) const;

    static tags CommonTags;
    static std::string const ProcessId;
    static int const RequestTimeOutMs;
    static std::string const LanguageFamily;

    // TODO: this should be passed in the constructor to avoid overwriting
    //       the .pprof generated by the managed side
    static std::string const RequestFileName;
    static std::string const ProfilePeriodType;
    static std::string const ProfilePeriodUnit;

    fs::path _pprofOutputPath;

    std::vector<ddprof_ffi_Location> _locations;
    std::vector<ddprof_ffi_Line> _lines;
    std::string _agentUrl;
    std::size_t _locationsAndLinesSize;

    // for each application, keep track of a profile, a samples count since the last export and an export count
    std::unordered_map<std::string_view, ProfileInfo> _perAppInfo;
    ddprof_ffi_EndpointV3 _endpoint;
    Tags _exporterBaseTags;
    IApplicationStore* const _applicationStore;

    std::mutex _perAppInfoLock;

public:  // for tests
    static std::string GetEnabledProfilersTag(IEnabledProfilers* enabledProfilers);
};