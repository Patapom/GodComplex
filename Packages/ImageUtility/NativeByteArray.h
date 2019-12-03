#pragma once

using namespace System;

namespace ImageUtility {

	// This class is a simple wrapper for native byte* that can be easily converted to and from a managed byte[]
	// The image utility library uses this intermediary wrapper to carry native pointers around without having to
	//	convert to and from byte[] every time but only when actually required by a native method
	//
	public ref class NativeByteArray {
	private:
		int			m_length;
		IntPtr		m_nativePointer;
		bool		m_freeOnExit;		// True to FreeHGlobal, false to delete[]...

	public:	// PROPERTIES
		property int					Length {
			int					get() { return m_length; }
		}
		property IntPtr					AsBytePointer {
			IntPtr				get() { return m_nativePointer; }
		}
		property cli::array< Byte >^	AsByteArray {
			cli::array< Byte >^	get() {
				// Copy to Byte[]
				cli::array< Byte >^	managedBuffer = gcnew cli::array< Byte >( m_length );
				System::Runtime::InteropServices::Marshal::Copy( m_nativePointer, managedBuffer, 0, m_length );

				return managedBuffer;
			}
		}

	public:	// METHODS
		NativeByteArray()
			: m_length( 0 )
			, m_nativePointer( nullptr )
			, m_freeOnExit( false ) {
		}

		// This constructor transfers the responsibility of deleting the array to this object
		NativeByteArray( NativeByteArray^ _other ) {
			m_length = _other->m_length;
			m_nativePointer = _other->m_nativePointer;
			m_freeOnExit = _other->m_freeOnExit;

			// Other must release the responsibility!
			_other->m_length = 0;
			_other->m_nativePointer = IntPtr::Zero;
			_other->m_freeOnExit = false;
		}

		// This constructor transfers the responsibility of deleting the array to this object
		NativeByteArray( int _length, void* _nativePointer ) {
			m_length = _length;
			m_nativePointer = IntPtr( _nativePointer );
			m_freeOnExit = false;	// Delete[] on exit
		}

		NativeByteArray( cli::array< Byte >^ _managedArray ) {
			m_length = _managedArray->Length;

			// Copy to Byte*
//			m_nativePointer = new Byte[m_length];
			m_nativePointer = System::Runtime::InteropServices::Marshal::AllocHGlobal( m_length );
			System::Runtime::InteropServices::Marshal::Copy( _managedArray, 0, m_nativePointer, m_length );

			m_freeOnExit = true;	// FreeHGlobal on exit
		}
		~NativeByteArray() {
			if ( m_nativePointer != IntPtr::Zero ) {
				if ( m_freeOnExit ) {
					System::Runtime::InteropServices::Marshal::FreeHGlobal( m_nativePointer );
				} else {
					delete[] m_nativePointer.ToPointer();
				}
			}
		}
	};
}
