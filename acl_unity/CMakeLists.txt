cmake_minimum_required (VERSION 3.2)
project(acl_unity CXX)

set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} "${PROJECT_SOURCE_DIR}/acl/cmake")

set(USE_SIMD_INSTRUCTIONS TRUE)

include(CMakeUtils)
include(CMakeCompiler)
include(CMakePlatforms)

set(CMAKE_CXX_STANDARD 11)

include_directories("${PROJECT_SOURCE_DIR}/acl/includes")
include_directories("${PROJECT_SOURCE_DIR}/acl/external/rtm/includes")

# Grab all of our main source files
file(GLOB ALL_MAIN_SOURCE_FILES LIST_DIRECTORIES false
	${PROJECT_SOURCE_DIR}/*.cpp)

create_source_groups("${ALL_MAIN_SOURCE_FILES}" ${PROJECT_SOURCE_DIR})

add_library(${PROJECT_NAME} SHARED ${ALL_MAIN_SOURCE_FILES})

setup_default_compiler_flags(${PROJECT_NAME})

if(MSVC)
	if(CPU_INSTRUCTION_SET MATCHES "arm64")
		# Exceptions are not enabled by default for ARM targets, enable them
		target_compile_options(${PROJECT_NAME} PRIVATE /EHsc)
	endif()
endif()

# Disable allocation tracking
add_definitions(-DACL_NO_ALLOCATOR_TRACKING)
