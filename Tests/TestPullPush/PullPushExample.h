//////////////////////////////////////////////////////////////////////
// PullPush.h - Interpolate a 2D array of arbitrary data.
//
// Copyright David K. McAllister, Oct. 1998.

// Provides scattered data interpolation on arbitrary 2D data.
// Uses two types:
// _WType is the type of the weight value. This is usually float.
// Some day I should try unsigned short or something.
//
// _Tp is the type of the data. This is usually float.
// Should also work with Pixel, Vector, unsigned int, etc.

#ifndef pullpush_h
#define pullpush_h

// #include <Util/Utils.h>
// #include <Math/MiscMath.h>
// #include <algorithm>

// #include <stdlib.h>
// #include <stdio.h>

template<class _WType>
inline _WType MIN1(_WType x)
{
    return (x) < 1.0f ? (x) : 1.0f;
}

// Could special case W >= 1.
template<class _Tp, class _WType>
inline void Composite(_Tp &R, _WType &W, _Tp tD, _WType tW)
{
    R = tD + W * (R - tD);
    W = MIN1(W + tW * (1 - W));
}

// Create an image smaller than the one I'm given, fill in as much of it
// as I know how. Then call recursively to have the rest of it filled in.
// Then use it to fill in the rest of me.
//
// All values of Weights must be <= 1.

template<class _Tp, class _WType>
void PullPush(_Tp *Data, _WType *Weights, int wid, int hgt)
{
#ifdef PP_DEBUG
    DoDebug("Inn", Data, Weights, wid, hgt);
#endif
    
    if(wid <= 1 && hgt <= 1)
        return;
    
    int widp = (wid+1) >> 1;
    int hgtp = (hgt+1) >> 1;
    
    cerr << "Shrinking to " << widp << "x" << hgtp << endl;
    
    // XXX What about initializing these?
    _Tp *Data1 = new _Tp[widp * hgtp];
    _WType *Weights1 = new _WType[widp * hgtp];
    
    // Make the smaller level.
    int x, y;
    for(y=0; y<hgtp; y++) {
        for(x=0; x<widp; x++) {
            // Deal with the 9 pixels affecting this one.
            int x2 = x<<1;
            int y2 = y<<1;
            
            _WType w = Weights[y2*wid+x2];
            _WType Wgt = w;
            _Tp Dat = Data[y2*wid+x2] * w;
            
            if(x2>0) {
                w = Weights[y2*wid+(x2-1)] / 2;
                //if(TooFar(Data(x2-1, y2), Dat, Wgt)) w = 0;
                Wgt += w;
                Dat += Data[y2*wid+(x2-1)] * w;
                
                if(y2>0) {
                    w = Weights[(y2-1)*wid+(x2-1)] / 4;
                    //if(TooFar(Data(x2-1, y2-1), Dat, Wgt)) w = 0;
                    Wgt += w;
                    Dat += Data[(y2-1)*wid+(x2-1)] * w;
                }
                
                if(y2 < hgt-1) {
                    w = Weights[(y2+1)*wid+(x2-1)] / 4;
                    //if(TooFar(Data(x2-1, y2+1), Dat, Wgt)) w = 0;
                    Wgt += w;
                    Dat += Data[(y2+1)*wid+(x2-1)] * w;
                }
            }
            
            if(y2 < hgt-1) {
                w = Weights[(y2+1)*wid+x2] / 2;
                //if(TooFar(Data(x2, y2+1), Dat, Wgt)) w = 0;
                Wgt += w;
                Dat += Data[(y2+1)*wid+x2] * w;
            }
            
            if(y2>0) {
                w = Weights[(y2-1)*wid+x2] / 2;
                //if(TooFar(Data(x2, y2-1), Dat, Wgt)) w = 0;
                Wgt += w;
                Dat += Data[(y2-1)*wid+x2] * w;
            }
            
            if(x2 < wid-1) {
                w = Weights[y2*wid+(x2+1)] / 2;
                //if(TooFar(Data(x2+1, y2), Dat, Wgt)) w = 0;
                Wgt += w;
                Dat += Data[y2*wid+(x2+1)] * w;
                
                if(y2>0) {
                    w = Weights[(y2-1)*wid+(x2+1)] / 4;
                    //if(TooFar(Data(x2+1, y2-1), Dat, Wgt)) w = 0;
                    Wgt += w;
                    Dat += Data[(y2-1)*wid+(x2+1)] * w;
                }
                
                if(y2 < hgt-1) {
                    w = Weights[(y2+1)*wid+(x2+1)] / 4;
                    //if(TooFar(Data(x2+1, y2+1), Dat, Wgt)) w = 0;
                    Wgt += w;
                    Dat += Data[(y2+1)*wid+(x2+1)] * w;
                }
            }
            
            // Scale the data by the weights.
            if(Wgt != 0) {
                Dat = Dat / Wgt;
                
                Data1[y*widp+x] = Dat;
                Weights1[y*widp+x] = MIN1(Wgt);
            } else {
                Data1[y*widp+x] = 0;
                Weights1[y*widp+x] = 0;
            }
        }
    }
    
    // Now that I've splatted onto the smaller level,
    // have the rest of it filled in.
    PullPush(Data1, Weights1, widp, hgtp);
    
    // Now use the smaller image to fill me in.
    for(y=0; y<hgt; y++) {
        int y1 = y>>1;
        
        // For even rows.
        for(x=0; x<wid; x++) {
            int x1 = x>>1;
            
            // An even pixel. // No need to mult/div by weight.
            _WType tw = Weights1[y1*widp+x1];
            _Tp tD = Data1[y1*widp+x1];
            
            Composite(Data[y*wid+x], Weights[y*wid+x], tD, tw);
            
            x++;
            if(x>=wid) break;
            
            // An odd pixel.
            _WType w = Weights1[y1*widp+x1] / 2;
            tw = w;
            tD = w * Data1[y1*widp+x1];
            
            if(x1+1 < widp) {
                w = Weights1[y1*widp+(x1+1)] / 2;
                //if(TooFar(Data1(x1+1, y1), tD, tw)) w = 0;
                tw += w;
                tD += w * Data1[y1*widp+(x1+1)];
            }
            
            tD /= tw;
            
            Composite(Data[y*wid+x], Weights[y*wid+x], tD, tw);
        }
        
        y++;
        if(y>=hgt) break;
        
        // For odd rows.
        for(x=0; x<wid; x++) {
            int x1 = x>>1;
            
            // An even pixel.
            _WType w = Weights1[y1*widp+x1] / 2;
            _WType tw = w;
            _Tp tD = w * Data1[y1*widp+x1];
            
            if(y1+1 < hgtp)
            {
                w = Weights1[(y1+1)*widp+x1] / 2;
                //if(TooFar(Data1[(y1+1)*widp+x1], tD, tw)) w = 0;
                tw += w;
                tD += w * Data1[(y1+1)*widp+x1];
            }
            
            tD /= tw;
            
            Composite(Data[y*wid+x], Weights[y*wid+x], tD, tw);
            
            x++;
            if(x>=wid) break;
            
            // An odd pixel.
            w = Weights1[y1*widp+x1] / 4;
            tw = w;
            tD = w * Data1[y1*widp+x1];
            
            if(x1+1 < widp) {
                w = Weights1[y1*widp+(x1+1)] / 4;
                //if(TooFar(Data1(x1+1, y1), tD, tw)) w = 0;
                tw += w;
                tD += w * Data1[y1*widp+(x1+1)];
                
                if(y1+1 < hgtp) {
                    w = Weights1[(y1+1)*widp+(x1+1)] / 4;
                    //if(TooFar(Data1(x1+1, y1+1), tD, tw)) w = 0;
                    tw += w;
                    tD += w * Data1[(y1+1)*widp+(x1+1)];
                }
            }
            
            if(y1+1 < hgtp) {
                w = Weights1[(y1+1)*widp+x1] / 4;
                //if(TooFar(Data1(x1, y1+1), tD, tw)) w = 0;
                tw += w;
                tD += w * Data1[(y1+1)*widp+x1];
            }
            
            tD /= tw;
            
            Composite(Data[y*wid+x], Weights[y*wid+x], tD, tw);
        }
    }
    
    delete [] Data1;
    delete [] Weights1;
    
#ifdef PP_DEBUG
    DoDebug("Out", Data, Weights, wid, hgt);
#endif
    
    cerr << "Finished filling in the " << wid << "x" << hgt << endl;
}

#endif
