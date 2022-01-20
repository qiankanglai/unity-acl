
#include <string>
#include <vector>

#include "acl/decompression/decompress.h"

using default_decompression_type = acl::decompression_context<acl::decompression_settings>;

struct my_track_writer : public acl::track_writer
{
	my_track_writer(void* _qvvf, char* _flags): qvvf((rtm::qvvf*)_qvvf), flags(_flags)
	{
	}
	
	void RTM_SIMD_CALL write_rotation(uint32_t track_index, rtm::quatf_arg0 rotation)
	{
		qvvf[track_index].rotation = rotation;
		flags[track_index] |= 1;
	}

	//////////////////////////////////////////////////////////////////////////
	// Called by the decoder to write out a translation value for a specified bone index.
	void RTM_SIMD_CALL write_translation(uint32_t track_index, rtm::vector4f_arg0 translation)
	{
		qvvf[track_index].translation = translation;
		flags[track_index] |= 2;
	}

	//////////////////////////////////////////////////////////////////////////
	// Called by the decoder to write out a scale value for a specified bone index.
	void RTM_SIMD_CALL write_scale(uint32_t track_index, rtm::vector4f_arg0 scale)
	{
		qvvf[track_index].scale = scale;
		flags[track_index] |= 4;
	}

	// skip track with default value
	static constexpr acl::default_sub_track_mode get_default_rotation_mode() { return acl::default_sub_track_mode::skipped; }
	static constexpr acl::default_sub_track_mode get_default_translation_mode() { return acl::default_sub_track_mode::skipped; }
	static constexpr acl::default_sub_track_mode get_default_scale_mode() { return acl::default_sub_track_mode::skipped; }
	
	rtm::qvvf* qvvf;
	char* flags;
};

extern "C" __declspec(dllexport) bool IsCompressedTracksValid (void* buffer)
{
	const auto& tracks = acl::make_compressed_tracks(buffer, nullptr);
	return (tracks != nullptr && tracks->is_valid(false).empty());
}

extern "C" __declspec(dllexport) int GetNumTracks (void* buffer)
{
	const auto& tracks = acl::make_compressed_tracks(buffer, nullptr);
	if (tracks != nullptr)
		return tracks->get_num_tracks();
	return 0;
}

extern "C" __declspec(dllexport) float GetDuration (void* buffer)
{
	const auto& tracks = acl::make_compressed_tracks(buffer, nullptr);
	if (tracks != nullptr)
		return tracks->get_duration();
	return 0;
}

extern "C" __declspec(dllexport) const char* GetTrackName (void* buffer, int track_index)
{
	const auto& tracks = acl::make_compressed_tracks(buffer, nullptr);
	if (tracks != nullptr)
		return tracks->get_track_name(track_index);
	return nullptr;
}

extern "C" __declspec(dllexport) int GetParentTrackIndex (void* buffer, int track_index)
{
	const auto& tracks = acl::make_compressed_tracks(buffer, nullptr);
	if (tracks != nullptr)
		return tracks->get_parent_track_index(track_index);
	return acl::k_invalid_track_index;
}

extern "C" __declspec(dllexport) void* PrepareDecompressContext (void* buffer)
{
	const auto& tracks = acl::make_compressed_tracks(buffer, nullptr);
	if (!tracks)
	{
		return nullptr;
	}

	const auto context = new (_aligned_malloc(sizeof(default_decompression_type), 64))default_decompression_type();
	if (!context->initialize(*tracks))
	{
		((default_decompression_type*)context)->~decompression_context();
		_aligned_free(context);
		return nullptr;
	}
	
	return context;
}

extern "C" __declspec(dllexport) void ReleaseDecompressContext (void* context)
{
	if (context)
	{
		((default_decompression_type*)context)->~decompression_context();
		_aligned_free(context);
	}
}

extern "C" __declspec(dllexport) void SeekInContext (void* context, float sample_time, int rounding_policy)
{
	if (context)
	{
		((default_decompression_type*)context)->seek(sample_time, (acl::sample_rounding_policy)rounding_policy);
	}
}

extern "C" __declspec(dllexport) void DecompressTracks (void* context, void* result, char* flags)
{
	if (context)
	{
		int num_tracks = ((default_decompression_type*)context)->get_compressed_tracks()->get_num_tracks();
		memset(result, 0, num_tracks * 12 * sizeof(float));
		memset(flags, 0, num_tracks * sizeof(char));
		my_track_writer writer(result, flags);
		((default_decompression_type*)context)->decompress_tracks(writer);
	}
}
