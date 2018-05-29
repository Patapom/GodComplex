cmake_minimum_required (VERSION 2.4)
if (COMMAND cmake_policy)
    cmake_policy (SET CMP0003 NEW)
endif ()

project (AxF-SDK-Samples)


#---------------------------------------------------------------------------
# define names of sample applications here (so that they can be changed more easily)

set (AXF_DECODE_APP AxFDecode)


#---------------------------------------------------------------------------
# macros for library search

macro (xrite_package_found_message basename)
    if (${basename}_INCLUDE_DIRS)
        message (STATUS "-- ${basename} include dirs:  \t ${${basename}_INCLUDE_DIRS}")
    endif ()
    if (${basename}_LIBRARIES)
        message (STATUS "-- ${basename} libraries:     \t ${${basename}_LIBRARIES}")
    endif ()
    if (NOT ${basename}_FOUND)
        message ("-- Error: ${basename} was not found! - Consider adding suitable library/include search paths to the")
        message ("          cmake-gui configurable options CUSTOM_INCLUDE_SEARCH_PATH and CUSTOM_LIBRARY_SEARCH_PATH.")
        message ("")
    endif ()
endmacro ()

# copied from CMake 3.0's SelectLibraryConfigurations.cmake:
macro (select_library_configurations basename)
    if (NOT ${basename}_LIBRARY_RELEASE)
        set (${basename}_LIBRARY_RELEASE "${basename}_LIBRARY_RELEASE-NOTFOUND" CACHE FILEPATH "Path to a library.")
    endif ()
    if (NOT ${basename}_LIBRARY_DEBUG)
        set (${basename}_LIBRARY_DEBUG "${basename}_LIBRARY_DEBUG-NOTFOUND" CACHE FILEPATH "Path to a library.")
    endif ()

    if (${basename}_LIBRARY_DEBUG AND ${basename}_LIBRARY_RELEASE AND
           NOT ${basename}_LIBRARY_DEBUG STREQUAL ${basename}_LIBRARY_RELEASE AND
           (CMAKE_CONFIGURATION_TYPES OR CMAKE_BUILD_TYPE))
        # if the generator supports configuration types or CMAKE_BUILD_TYPE
        # is set, then set optimized and debug options.
        set (${basename}_LIBRARY "")
        foreach (_libname IN LISTS ${basename}_LIBRARY_RELEASE)
            list (APPEND ${basename}_LIBRARY optimized "${_libname}")
        endforeach ()
        foreach (_libname IN LISTS ${basename}_LIBRARY_DEBUG)
            list (APPEND ${basename}_LIBRARY debug "${_libname}")
        endforeach ()
    elseif (${basename}_LIBRARY_RELEASE)
        set (${basename}_LIBRARY ${${basename}_LIBRARY_RELEASE})
    elseif (${basename}_LIBRARY_DEBUG)
        set (${basename}_LIBRARY ${${basename}_LIBRARY_DEBUG})
    else()
        set (${basename}_LIBRARY "${basename}_LIBRARY-NOTFOUND")
    endif()

    set (${basename}_LIBRARIES "${${basename}_LIBRARY}")

    if (${basename}_LIBRARY)
        set (${basename}_FOUND TRUE)
    endif()

    mark_as_advanced (${basename}_LIBRARY_RELEASE ${basename}_LIBRARY_DEBUG)
endmacro()

macro (xrite_set_package_vars basename)
    select_library_configurations (${basename})
    mark_as_advanced (${basename}_INCLUDE_DIR)
    if (${basename}_INCLUDE_DIR)
        set (${basename}_INCLUDE_DIRS ${${basename}_INCLUDE_DIR})
    else ()
        unset (${basename}_FOUND)
    endif ()
    xrite_package_found_message (${basename})
endmacro()

macro (xrite_clear_dependent_cache_vars v)
    if (DEFINED _${v}_LAST AND NOT (${v} STREQUAL _${v}_LAST))
        foreach (DEPENDENT_VAR ${ARGN})
            unset (${DEPENDENT_VAR} CACHE)
        endforeach ()
    endif ()
    set (_${v}_LAST "${${v}}" CACHE INTERNAL "Last used ${v} value.")
endmacro ()


#---------------------------------------------------------------------------
# configure build output directory

set (SYSTEM_DIR_NAME "${CMAKE_SYSTEM_NAME}")
if (SYSTEM_DIR_NAME STREQUAL "Darwin")
    set (SYSTEM_DIR_NAME "MacOSX")
endif ()
if ((CMAKE_VS_PLATFORM_NAME AND CMAKE_VS_PLATFORM_NAME STREQUAL "x64") OR (CMAKE_SIZEOF_VOID_P EQUAL 8 AND CMAKE_SYSTEM_PROCESSOR MATCHES "(x86|i.86|AMD)"))
    set (SYSTEM_DIR_NAME "${SYSTEM_DIR_NAME}.x64")
else ()
    set (SYSTEM_DIR_NAME "${SYSTEM_DIR_NAME}.${CMAKE_SYSTEM_PROCESSOR}")
endif ()

set (CMAKE_RUNTIME_OUTPUT_DIRECTORY "${CMAKE_SOURCE_DIR}/../${SYSTEM_DIR_NAME}/bin")

# if applicable, use per-configuration postfixes for the executables (see below) instead of separate subdirecties
# below the bin directory (recommendable for Windows to make sure that the AxFDecoding DLLs will be found):
if (CMAKE_CONFIGURATION_TYPES)
    foreach (Config ${CMAKE_CONFIGURATION_TYPES})
        string (TOUPPER ${Config} CONFIG)
        set (CMAKE_RUNTIME_OUTPUT_DIRECTORY_${CONFIG} ${CMAKE_RUNTIME_OUTPUT_DIRECTORY})
    endforeach ()
endif ()


#---------------------------------------------------------------------------
# define cmake-gui options

# defaults:
set (DEFAULT_CUSTOM_INCLUDE_SEARCH_PATH "../include")
set (DEFAULT_CUSTOM_LIBRARY_SEARCH_PATH "../${SYSTEM_DIR_NAME}/lib")

if (WIN32)
    set (BOOST_DYN_LINK_DEFAULT OFF)
else ()
    set (BOOST_DYN_LINK_DEFAULT ON)
endif ()


# user options:
set (CUSTOM_INCLUDE_SEARCH_PATH "${DEFAULT_CUSTOM_INCLUDE_SEARCH_PATH}" CACHE STRING "search these directories for include files (in preference over standard locations known to CMake) - semicolon separated list (may include wildcards)")
set (CUSTOM_LIBRARY_SEARCH_PATH "${DEFAULT_CUSTOM_LIBRARY_SEARCH_PATH}" CACHE STRING "search these directories for lib files (in preference over standard locations known to CMake) - semicolon separated list (may include wildcards)")

set (BOOST_DYN_LINK       ${BOOST_DYN_LINK_DEFAULT} CACHE BOOL "link Boost as dynamic/shared library rather than as static library")

set (FREEIMAGE_DEPENDENCY ON                        CACHE BOOL "enable FreeImage dependency of the sample (if disabled, the sample will not write any image files to disk)")
set (FREEIMAGE_DYN_LINK   ON                        CACHE BOOL "link FreeImage as dynamic/shared library rather than as static library")


# if the above cache variables are changed (in case of multiple runs through cmake-gui), clear the respective dependent advanced cache variables:
xrite_clear_dependent_cache_vars (CUSTOM_INCLUDE_SEARCH_PATH AxFDecoding_INCLUDE_DIR Boost_INCLUDE_DIR FreeImage_INCLUDE_DIR)
xrite_clear_dependent_cache_vars (CUSTOM_LIBRARY_SEARCH_PATH AxFDecoding_LIBRARY_RELEASE AxFDecoding_LIBRARY_DEBUG Boost_LIBRARY_DIR Boost_LIBRARY_DIR_RELEASE Boost_LIBRARY_DIR_DEBUG FreeImage_LIBRARY_RELEASE FreeImage_LIBRARY_DEBUG)
xrite_clear_dependent_cache_vars (BOOST_DYN_LINK             Boost_INCLUDE_DIR Boost_LIBRARY_DIR Boost_LIBRARY_DIR_RELEASE Boost_LIBRARY_DIR_DEBUG)
xrite_clear_dependent_cache_vars (FREEIMAGE_DEPENDENCY       FreeImage_INCLUDE_DIR FreeImage_LIBRARY_RELEASE FreeImage_LIBRARY_DEBUG)
xrite_clear_dependent_cache_vars (FREEIMAGE_DYN_LINK         FreeImage_INCLUDE_DIR FreeImage_LIBRARY_RELEASE FreeImage_LIBRARY_DEBUG)

# hide some less useful standard CMake options from the gui:
mark_as_advanced (CMAKE_BACKWARDS_COMPATIBILITY CMAKE_INSTALL_PREFIX EXECUTABLE_OUTPUT_PATH LIBRARY_OUTPUT_PATH)


#---------------------------------------------------------------------------
# search required libraries

# setup custom include/library search paths if applicable:
if (WIN32)
    file (TO_CMAKE_PATH "${CUSTOM_INCLUDE_SEARCH_PATH}" CUSTOM_INCLUDE_SEARCH_PATH)
    file (TO_CMAKE_PATH "${CUSTOM_LIBRARY_SEARCH_PATH}" CUSTOM_LIBRARY_SEARCH_PATH)
endif ()
file (GLOB CUSTOM_INCLUDE_SEARCH_PATH ${CUSTOM_INCLUDE_SEARCH_PATH})
file (GLOB CUSTOM_LIBRARY_SEARCH_PATH ${CUSTOM_LIBRARY_SEARCH_PATH})
set (CMAKE_INCLUDE_PATH ${CUSTOM_INCLUDE_SEARCH_PATH} ${CMAKE_INCLUDE_PATH})
set (CMAKE_LIBRARY_PATH ${CUSTOM_LIBRARY_SEARCH_PATH} ${CMAKE_LIBRARY_PATH})
message (STATUS "-- Custom include search path (expanded):   ${CMAKE_INCLUDE_PATH}")
message (STATUS "-- Custom library search path (expanded):   ${CMAKE_LIBRARY_PATH}")

# search for the AxF Decoding SDK:
find_path (AxFDecoding_INCLUDE_DIR AxF/decoding/AxF_basic_io.h)
find_library (AxFDecoding_LIBRARY_RELEASE AxFDecoding)
find_library (AxFDecoding_LIBRARY_DEBUG   AxFDecoding_d)
xrite_set_package_vars (AxFDecoding)

# search for Boost:
set (BOOST_REQ_COMPONENTS filesystem program_options regex system)
if (CMAKE_CXX_COMPILER_ID MATCHES "Clang")
    # note: For most compilers, CMake's FindBoost module automatically determines Boost's compiler tag that is matching the current compiler, but not for Clang yet:
    set (Boost_COMPILER -clang)
    if (APPLE)
        set (Boost_COMPILER ${Boost_COMPILER}-darwin)
    endif ()
    exec_program (${CMAKE_CXX_COMPILER} ARGS ${CMAKE_CXX_COMPILER_ARG1} -dumpversion OUTPUT_VARIABLE COMPILER_VERSION)
    string (REGEX REPLACE "([0-9])\\.([0-9])(\\.[0-9])?" "\\1\\2" COMPILER_VERSION ${COMPILER_VERSION})
    set (Boost_COMPILER ${Boost_COMPILER}${COMPILER_VERSION})
endif ()
set (Boost_DETAILED_FAILURE_MSG ON)
if (NOT BOOST_DYN_LINK)
    set (Boost_USE_STATIC_LIBS ON)
endif ()
find_package (Boost ${BOOST_REQ_MIN_VER} REQUIRED COMPONENTS ${BOOST_REQ_COMPONENTS})
xrite_package_found_message (Boost)

# search for FreeImage (if FREEIMAGE_DEPENDENCY is enabled):
if (FREEIMAGE_DEPENDENCY)
    find_path (FreeImage_INCLUDE_DIR FreeImage.h PATH_SUFFIXES Source)
    if (WIN32)
        if (FREEIMAGE_DYN_LINK)
            set (FreeImage_PREFERRED_LIB_NAME FreeImage_dll)
        else ()
            set (FreeImage_PREFERRED_LIB_NAME FreeImageLib)
        endif ()
    else ()
        set (FreeImage_PREFERRED_LIB_NAME freeimage)
        if (FREEIMAGE_DYN_LINK)
            set (CMAKE_FIND_LIBRARY_SUFFIXES ${CMAKE_SHARED_LIBRARY_SUFFIX})
        else ()
            set (CMAKE_FIND_LIBRARY_SUFFIXES ${CMAKE_STATIC_LIBRARY_SUFFIX})
        endif ()
    endif ()
    find_library (FreeImage_LIBRARY_RELEASE NAMES ${FreeImage_PREFERRED_LIB_NAME}                                    FreeImage)
    find_library (FreeImage_LIBRARY_DEBUG   NAMES ${FreeImage_PREFERRED_LIB_NAME}d ${FreeImage_PREFERRED_LIB_NAME}_d FreeImaged FreeImage_d)
    xrite_set_package_vars (FreeImage)
endif ()


#---------------------------------------------------------------------------
# set some recommendable (but not necessarily required) compiler options

# enable C++ 11 if the compiler supports it (relevant for GCC/Clang only):
set (CMAKE_CXX_STANDARD 11)    # supported since CMake 3.1, but it seems that some previous CMake versions already enable C++ 11 by default; otherwise the explicit alternative below may be used (here for C++ 0x)
#include (TestCXXAcceptsFlag)
#if (CMAKE_COMPILER_IS_GNUCXX)
#    check_cxx_accepts_flag ("-std=c++0x" SUPPORTS_CPP0X)
#    if (SUPPORTS_CPP0X)
#        set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++0x")
#    endif ()
#endif ()

if (MSVC)
    # this adds the "Unicode Support" property sheet to the VS project (in case of VS >= 2010):
    add_definitions (-D_UNICODE -DUNICODE)
endif ()


#---------------------------------------------------------------------------
# pass some of the options to the compiler as defines

# defines used in the sample code only:
if (BOOST_DYN_LINK)
    add_definitions (-DAXFSAMPLES_BOOST_DYN_LINK)
endif ()
if (FREEIMAGE_DEPENDENCY)
    add_definitions (-DAXFSAMPLES_FREEIMAGE_DEPENDENCY)
endif ()
if (FREEIMAGE_DYN_LINK)
    add_definitions (-DAXFSAMPLES_FREEIMAGE_DYN_LINK)
endif ()


#---------------------------------------------------------------------------
# AxFDecode sample - setup executable target and its dependencies

set (AXF_DECODE_FILES
    main.cpp
)

include_directories (${AxFDecoding_INCLUDE_DIRS})
include_directories (${Boost_INCLUDE_DIRS})
if (FREEIMAGE_DEPENDENCY)
    include_directories (${FreeImage_INCLUDE_DIRS})
endif ()

add_executable (${AXF_DECODE_APP} ${AXF_DECODE_FILES})

target_link_libraries (${AXF_DECODE_APP} ${AxFDecoding_LIBRARIES})
target_link_libraries (${AXF_DECODE_APP} ${Boost_LIBRARIES})
if (FREEIMAGE_DEPENDENCY)
    target_link_libraries (${AXF_DECODE_APP} ${FreeImage_LIBRARIES})
endif ()

# if applicable, use per-configuration postfixes for the executables (see comment above):
if (CMAKE_CONFIGURATION_TYPES)
    foreach (Config ${CMAKE_CONFIGURATION_TYPES})
        string (TOUPPER ${Config} CONFIG)
        set_target_properties (${AXF_DECODE_APP} PROPERTIES ${CONFIG}_POSTFIX _${Config})
    endforeach ()
endif ()
