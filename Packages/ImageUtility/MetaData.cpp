#include "stdafx.h"

#include "MetaData.h"
#include "ImageFile.h"

using namespace ImageUtility;

MetaData::MetaData( ImageFile^ _owner ) {
	m_nativeObject = &_owner->m_nativeObject->GetMetadata();
}
