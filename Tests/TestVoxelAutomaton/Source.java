// From https://bitbucket.org/BWerness/voxel-automata-terrain/overview
//
import java.math.BigInteger;
import java.util.*;
import com.jogamp.newt.opengl.GLWindow;

// size of the universe
int L = 6;
int K = (1<<L)+1;

// the universe
int[][][] state = new int[K][K][K];

// rules
int[][] cubeRule = new int[9][9];
int[][] faceRule = new int[7][7];
int[][] edgeRule = new int[7][7];

// set to be non-zero for the rule to become stochastic
float flipP = 0.0; 

// fill the center of a cube
void evalCube(int i, int j, int k, int w) {
  if ((i < 0) || (j < 0) || (k < 0) || (i+w >= K) || (j+w >= K) || (k+w >= K)) return;
  int idx1 = (state[i][j][k]==1?1:0) + (state[i+w][j][k]==1?1:0) + (state[i][j+w][k]==1?1:0) + (state[i+w][j+w][k]==1?1:0) +
             (state[i][j][k+w]==1?1:0) + (state[i+w][j][k+w]==1?1:0) + (state[i][j+w][k+w]==1?1:0) + (state[i+w][j+w][k+w]==1?1:0);
  int idx2 = (state[i][j][k]==2?1:0) + (state[i+w][j][k]==2?1:0) + (state[i][j+w][k]==2?1:0) + (state[i+w][j+w][k]==2?1:0) +
             (state[i][j][k+w]==2?1:0) + (state[i+w][j][k+w]==2?1:0) + (state[i][j+w][k+w]==2?1:0) + (state[i+w][j+w][k+w]==2?1:0);
  state[i+w/2][j+w/2][k+w/2] = cubeRule[idx1][idx2];
  if ((random(1.0) < flipP) && (state[i+w/2][j+w/2][k+w/2] != 0)) {
    state[i+w/2][j+w/2][k+w/2] = 3 - state[i+w/2][j+w/2][k+w/2];
  }
}

// fill a face
void f1(int i, int j, int k, int w) {
  if ((i < 0) || (j < 0) || (k-w/2 < 0) || (i+w >= K) || (j+w >= K) || (k+w/2 >= K)) return;
  int idx1 = (state[i][j][k]==1?1:0) + (state[i+w][j][k]==1?1:0) + (state[i][j+w][k]==1?1:0) + (state[i+w][j+w][k]==1?1:0) +
             (state[i+w/2][j+w/2][k-w/2]==1?1:0) + (state[i+w/2][j+w/2][k+w/2]==1?1:0);
  int idx2 = (state[i][j][k]==2?1:0) + (state[i+w][j][k]==2?1:0) + (state[i][j+w][k]==2?1:0) + (state[i+w][j+w][k]==2?1:0) +
             (state[i+w/2][j+w/2][k-w/2]==2?1:0) + (state[i+w/2][j+w/2][k+w/2]==2?1:0);
  state[i+w/2][j+w/2][k] = faceRule[idx1][idx2];
  
  if ((random(1.0) < flipP) && (state[i+w/2][j+w/2][k] != 0)) {
    state[i+w/2][j+w/2][k] = 3 - state[i+w/2][j+w/2][k];
  }
}

// fill a face
void f2(int i, int j, int k, int w) {
  if ((i < 0) || (j-w/2 < 0) || (k < 0) || (i+w >= K) || (j+w/2 >= K) || (k+w >= K)) return;
  int idx1 = (state[i][j][k]==1?1:0) + (state[i+w][j][k]==1?1:0) + (state[i][j][k+w]==1?1:0) + (state[i+w][j][k+w]==1?1:0) +
             (state[i+w/2][j-w/2][k+w/2]==1?1:0) + (state[i+w/2][j+w/2][k+w/2]==1?1:0);
  int idx2 = (state[i][j][k]==2?1:0) + (state[i+w][j][k]==2?1:0) + (state[i][j][k+w]==2?1:0) + (state[i+w][j][k+w]==2?1:0) +
             (state[i+w/2][j-w/2][k+w/2]==2?1:0) + (state[i+w/2][j+w/2][k+w/2]==2?1:0);
  state[i+w/2][j][k+w/2] = faceRule[idx1][idx2];
  
  if ((random(1.0) < flipP) && (state[i+w/2][j][k+w/2] != 0)) {
    state[i+w/2][j][k+w/2] = 3 - state[i+w/2][j][k+w/2];
  }
}

// fill a face
void f3(int i, int j, int k, int w) {
  if ((i-w/2 < 0) || (j < 0) || (k < 0) || (i+w/2 >= K) || (j+w >= K) || (k+w >= K)) return;
  int idx1 = (state[i][j][k]==1?1:0) + (state[i][j][k+w]==1?1:0) + (state[i][j+w][k]==1?1:0) + (state[i][j+w][k+w]==1?1:0) +
             (state[i-w/2][j+w/2][k+w/2]==1?1:0) + (state[i+w/2][j+w/2][k+w/2]==1?1:0);
  int idx2 = (state[i][j][k]==2?1:0) + (state[i][j][k+w]==2?1:0) + (state[i][j+w][k]==2?1:0) + (state[i][j+w][k+w]==2?1:0) +
             (state[i-w/2][j+w/2][k+w/2]==2?1:0) + (state[i+w/2][j+w/2][k+w/2]==2?1:0);
  state[i][j+w/2][k+w/2] = faceRule[idx1][idx2];
  
  if ((random(1.0) < flipP) && (state[i][j+w/2][k+w/2] != 0)) {
    state[i][j+w/2][k+w/2] = 3 - state[i][j+w/2][k+w/2];
  }
}

// fill a face
void f4(int i, int j, int k, int w) {
  f1(i,j,k+w,w);  
}

// fill a face
void f5(int i, int j, int k, int w) {
  f1(i,j+w,k,w);  
}

// fill a face
void f6(int i, int j, int k, int w) {
  f1(i+w,j,k,w);  
}

// fill every face
void evalFaces(int i, int j, int k, int w) {
  f1(i,j,k,w);
  f2(i,j,k,w);
  f3(i,j,k,w);
  f4(i,j,k,w);
  f5(i,j,k,w);
  f6(i,j,k,w);
}

// fill an edge
void e1(int i, int j, int k, int w) {
  if ((i < 0) || (j-w/2 < 0) || (k-w/2 < 0) || (i+w >= K) || (j+w/2 >= K) || (k+w/2 >= K)) return;
  int idx1 = (state[i][j][k]==1?1:0) + (state[i+w][j][k]==1?1:0) + (state[i+w/2][j-w/2][k]==1?1:0) + (state[i+w/2][j+w/2][k]==1?1:0) +
             (state[i+w/2][j][k+w/2]==1?1:0) + (state[i+w/2][j][k-w/2]==1?1:0);
  int idx2 = (state[i][j][k]==2?1:0) + (state[i+w][j][k]==2?1:0) + (state[i+w/2][j-w/2][k]==2?1:0) + (state[i+w/2][j+w/2][k]==2?1:0) +
             (state[i+w/2][j][k+w/2]==2?1:0) + (state[i+w/2][j][k-w/2]==2?1:0);
  state[i+w/2][j][k] = edgeRule[idx1][idx2];
  
  if ((random(1.0) < flipP) && (state[i+w/2][j][k] != 0)) {
    state[i+w/2][j][k] = 3 - state[i+w/2][j][k];
  }
}

// fill an edge
void e2(int i, int j, int k, int w) {
  e1(i,j+w,k,w);
}

// fill an edge
void e3(int i, int j, int k, int w) {
  e1(i,j,k+w,w);
}

// fill an edge
void e4(int i, int j, int k, int w) {
  e1(i,j+w,k+w,w);
}

// fill an edge
void e5(int i, int j, int k, int w) {
  e1(i-w/2,j+w/2,k,w);
}

// fill an edge
void e6(int i, int j, int k, int w) {
  e1(i+w/2,j+w/2,k,w);
}

// fill an edge
void e7(int i, int j, int k, int w) {
  e1(i-w/2,j+w/2,k+w,w);
}

// fill an edge
void e8(int i, int j, int k, int w) {
  e1(i+w/2,j+w/2,k+w,w);
}

// fill all edges
void evalEdges(int i, int j, int k, int w) {
  e1(i,j,k,w);
  e2(i,j,k,w);
  e3(i,j,k,w);
  e4(i,j,k,w);
  e5(i,j,k,w);
  e6(i,j,k,w);
  e7(i,j,k,w);
  e8(i,j,k,w);
}

// strength of the sky light
float lightStrength = 1.2;
// strength of the ambient light
float ambStrength = 0.2;

// the color of light emmited from each voxel
float[][][] rLight = new float[K][K][K];
float[][][] gLight = new float[K][K][K];
float[][][] bLight = new float[K][K][K];

float[] color0 = new float[]{255/255.0, 241/255.0, 224/255.0}; // sun light color
float[] color1 = new float[]{210/255.0 ,180/255.0, 140/255.0}; // Wikipedia Tan
float[] color2 = new float[]{143/255.0, 151/255.0, 121/255.0}; // Wikipedia Artichoke Green

// Make the global illumination by tracing upwards and seeing what is emitted from what you hit
void uniDirGI() {
  rLight = new float[K][K][K];
  gLight = new float[K][K][K];
  bLight = new float[K][K][K];
  
  for (int k = 0; k < K; k++) {
    for (int i = 0; i < K; i++) {
      for (int j = 0; j < K; j++) {
        rLight[i][j][k] = 0;
        gLight[i][j][k] = 0;
        bLight[i][j][k] = 0;
        if (state[i][j][k] != 0) {
          for (int di = -1; di <= 1; di++) {
            for (int dj = -1; dj <= 1; dj++) {
              int ci = i+di;
              int cj = j+dj;
              int ck = k-1;
              boolean hit = false;
              while ((ci >= 0) && (ci < K) && (cj >= 0) && (cj < K) && (ck >= 0)) {
                if (state[ci][cj][ck] != 0) {
                  rLight[i][j][k] += rLight[ci][cj][ck]/9;
                  gLight[i][j][k] += gLight[ci][cj][ck]/9;
                  bLight[i][j][k] += bLight[ci][cj][ck]/9;
                  
                  hit = true;
                  break;
                }
                ci += di;
                cj += dj;
                ck--;
              }
              if (!hit) {
                rLight[i][j][k] += color0[0]*lightStrength/9;
                gLight[i][j][k] += color0[1]*lightStrength/9;
                bLight[i][j][k] += color0[2]*lightStrength/9;
              }
            }  
          }
          
          if (state[i][j][k] == 1) {
            rLight[i][j][k] *= color1[0];
            gLight[i][j][k] *= color1[1];
            bLight[i][j][k] *= color1[2];
          }
          if (state[i][j][k] == 2) {
            rLight[i][j][k] *= color2[0];
            gLight[i][j][k] *= color2[1];
            bLight[i][j][k] *= color2[2];
          }
        }
      }
    }
  }
}

// Simple cheap ambient occlusion just by counting open neighbors
void addAmbient() {
  for (int i = 1; i < K-1; i++) {
    for (int j = 1; j < K-1; j++) {
      for (int k = 1; k < K-1; k++) {
        if (state[i][j][k] != 0) {
          int neighbors = (state[i-1][j][k]==0?1:0)+ (state[i+1][j][k]==0?1:0)+ (state[i][j-1][k]==0?1:0)+ (state[i][j+1][k]==0?1:0)+ (state[i][j][k-1]==0?1:0)+ (state[i][j][k+1]==0?1:0);
          float nrLight = neighbors/6.0;
          float ngLight = neighbors/6.0;
          float nbLight = neighbors/6.0;
          if (state[i][j][k] == 1) {
            nrLight *= color1[0];
            ngLight *= color1[1];
            nbLight *= color1[2];
          }
          if (state[i][j][k] == 2) {
            nrLight *= color2[0];
            ngLight *= color2[1];
            nbLight *= color2[2];
          }
          rLight[i][j][k] += ambStrength*nrLight;
          gLight[i][j][k] += ambStrength*ngLight;
          bLight[i][j][k] += ambStrength*nbLight;
        }
      }
    }
  }
}

// create a random rule with density lambda of filled states
void randomRule(float lambda) {
  for (int i = 0; i < 9; i++) {
    for (int j = 0; j < 9-i; j++) {
      if (((i == 0) && (j == 0)) || (random(1.0)>lambda)) cubeRule[i][j] = 0;
      else cubeRule[i][j] = int(random(2))+1;
    }
  }
  for (int i = 0; i < 7; i++) {
    for (int j = 0; j < 7-i; j++) {
      if (((i == 0) && (j == 0)) || (random(1.0)>lambda)) faceRule[i][j] = 0;
      else faceRule[i][j] = int(random(2))+1;
    }
  }
  for (int i = 0; i < 7; i++) {
    for (int j = 0; j < 7-i; j++) {
      if (((i == 0) && (j == 0)) || (random(1.0)>lambda)) edgeRule[i][j] = 0;
      else edgeRule[i][j] = int(random(2))+1;
    }
  }
  println(makeShortRule());
}

// create a rule based on a sample from the ising model where each one is selected with a certain weight depending on similarity to neighbors
void randomIsingRule(float beta, float mag) {
  for (int i = 0; i < 9; i++) {
    for (int j = 0; j < 9-i; j++) {
      float f0 = exp(beta*(8-(i+j)));
      float f1 = exp(beta*i+mag);
      float f2 = exp(beta*j+mag);
      float s = (f0+f1+f2);
      float r = random(s);
      if (r < f0) cubeRule[i][j] = 0;
      else if (r < f0+f1) cubeRule[i][j] = 1;
      else cubeRule[i][j] = 2;
    }
  }
  for (int i = 0; i < 7; i++) {
    for (int j = 0; j < 7-i; j++) {
      float f0 = exp(beta*(6-(i+j)));
      float f1 = exp(beta*i+mag);
      float f2 = exp(beta*j+mag);
      float s = (f0+f1+f2);
      float r = random(s);
      if (r < f0) faceRule[i][j] = 0;
      else if (r < f0+f1) faceRule[i][j] = 1;
      else faceRule[i][j] = 2;
    }
  }
  for (int i = 0; i < 7; i++) {
    for (int j = 0; j < 7-i; j++) {
      float f0 = exp(beta*(6-(i+j)));
      float f1 = exp(beta*i+mag);
      float f2 = exp(beta*j+mag);
      float s = (f0+f1+f2);
      float r = random(s);
      if (r < f0) edgeRule[i][j] = 0;
      else if (r < f0+f1) edgeRule[i][j] = 1;
      else edgeRule[i][j] = 2;
    }
  }
  println(makeShortRule());
}

// fill the bottom with random bits (the other sides are all empty)
void initState() {
  // fill just the bottom so we can see
  print("Randomizing IC...");
  for (int i = 0; i < K; i++) {
    for (int j = 0; j < K; j++) {
      state[i][j][K-1] = int(random(2))+1;  
    }
  }  
}

// double the size of the universe filling in new bits at random
void doubleState() {
  // double it filling new states with randomness (as long as it isn't too big)
  if (L >= 9) return;
  px *= 2;
  py *= 2;
  pz *= 2;
  L = L+1;
  print("Rescaling to " + L + "...");
  K = (1<<L)+1;
  int[][][] newState = new int[K][K][K];
  
  rLight = new float[K][K][K];
  gLight = new float[K][K][K];
  bLight = new float[K][K][K];
  for (int i = 0; i < K; i++) {
    for (int j = 0; j < K; j++) {
      if ((i%2 == 0) && (j%2 == 0)) newState[i][j][K-1] = state[i/2][j/2][(K-1)/2];
      else newState[i][j][K-1] = int(random(2))+1;
    }
  } 
  state = newState;
}

// cut the size of the universe in half
void halfState() {
  // double it filling new states with randomness (as long as it isn't too big)
  if (L <= 1) return;
  px /= 2;
  py /= 2;
  pz /= 2;
  L = L-1;
  print("Rescaling to " + L + "...");
  K = (1<<L)+1;
  int[][][] newState = new int[K][K][K];
  
  rLight = new float[K][K][K];
  gLight = new float[K][K][K];
  bLight = new float[K][K][K];
  for (int i = 0; i < K; i++) {
    for (int j = 0; j < K; j++) {
      newState[i][j][K-1] = state[2*i][2*j][2*(K-1)];
    }
  }
  state = newState;
}

// compute the full state of the universe, propegating in from the boundary
// also build it into textures for efficient drawing
void evalState() {
  print("Computing...");
  // do everything on all scales in order
  for (int w = K-1; w >= 2; w /= 2) {
    for (int i = 0; i < K-1; i+=w) {
      for (int j = 0; j < K-1; j+=w) {
        for (int k = 0; k < K-1; k+=w) {
          evalCube(i,j,k,w);
        }
      }
    }
    for (int i = 0; i < K-1; i+=w) {
      for (int j = 0; j < K-1; j+=w) {
        for (int k = 0; k < K-1; k+=w) {
          evalFaces(i,j,k,w);
        }
      }
    }
    for (int i = 0; i < K-1; i+=w) {
      for (int j = 0; j < K-1; j+=w) {
        for (int k = 0; k < K-1; k+=w) {
          evalEdges(i,j,k,w);
        }
      }
    }
  }
  // draw the dots to the PShape for efficiency
  print("Lighting...");
  uniDirGI();
  addAmbient();
  print("Building Textures...");
  makeTexes();
  println("Done.");
}

// A few things for display
PImage skyGradient;
PShape skySphere;
PShader smoothShader;

// initialize the window
void settings() {
  size(484,484,P3D);
}

// load eveything and get ready to draw
void setup() {
  skyGradient = loadImage("skyGradientHF.png");
  skySphere = createShape(SPHERE,1);
  skySphere.setTexture(skyGradient);
  skySphere.setStroke(false);
  
  String osString = System.getProperty("os.name", "generic").toLowerCase(Locale.ENGLISH);
  if (osString.contains("mac") || osString.contains("darwin"))
    smoothShader = loadShader("smoothMac.glsl");
  else
    smoothShader = loadShader("smoothWin.glsl");

  readShortRule("jxDFmQJZgXwLxmg83f9dQ5DSst");
  initState();
  evalState();
  
  hint(DISABLE_DEPTH_TEST);
  hint(DISABLE_TEXTURE_MIPMAPS);
  noSmooth();
}

// parameters for the rules
float lambda = 0.35;
float beta = 0.5;
float mag = 0.0;

// keep track of a few flags
boolean drawBack = true;
boolean[] pressedDir = new boolean[6];
boolean shifted = false;
boolean drawViewfinder = false;
boolean inkShade = false;
void keyPressed() {
  switch(key) {
    case '1': L = 1; print("New scale 1..."); K = (1<<L)+1; state = new int[K][K][K]; rLight = new float[K][K][K]; gLight = new float[K][K][K]; bLight = new float[K][K][K]; initState(); evalState(); px = K/2; py = K/2; pz = 3*K; theta = PI/2; phi = -PI/2; break;
    case '2': L = 2; print("New scale 2..."); K = (1<<L)+1; state = new int[K][K][K]; rLight = new float[K][K][K]; gLight = new float[K][K][K]; bLight = new float[K][K][K]; initState(); evalState(); px = K/2; py = K/2; pz = 3*K; theta = PI/2; phi = -PI/2; break;
    case '3': L = 3; print("New scale 3..."); K = (1<<L)+1; state = new int[K][K][K]; rLight = new float[K][K][K]; gLight = new float[K][K][K]; bLight = new float[K][K][K]; initState(); evalState(); px = K/2; py = K/2; pz = 3*K; theta = PI/2; phi = -PI/2; break;
    case '4': L = 4; print("New scale 4..."); K = (1<<L)+1; state = new int[K][K][K]; rLight = new float[K][K][K]; gLight = new float[K][K][K]; bLight = new float[K][K][K]; initState(); evalState(); px = K/2; py = K/2; pz = 3*K; theta = PI/2; phi = -PI/2; break;
    case '5': L = 5; print("New scale 5..."); K = (1<<L)+1; state = new int[K][K][K]; rLight = new float[K][K][K]; gLight = new float[K][K][K]; bLight = new float[K][K][K]; initState(); evalState(); px = K/2; py = K/2; pz = 3*K; theta = PI/2; phi = -PI/2; break;
    case '6': L = 6; print("New scale 6..."); K = (1<<L)+1; state = new int[K][K][K]; rLight = new float[K][K][K]; gLight = new float[K][K][K]; bLight = new float[K][K][K]; initState(); evalState(); px = K/2; py = K/2; pz = 3*K; theta = PI/2; phi = -PI/2; break;
    case '7': L = 7; print("New scale 7..."); K = (1<<L)+1; state = new int[K][K][K]; rLight = new float[K][K][K]; gLight = new float[K][K][K]; bLight = new float[K][K][K]; initState(); evalState(); px = K/2; py = K/2; pz = 3*K; theta = PI/2; phi = -PI/2; break;
    case '8': L = 8; print("New scale 8..."); K = (1<<L)+1; state = new int[K][K][K]; rLight = new float[K][K][K]; gLight = new float[K][K][K]; bLight = new float[K][K][K]; initState(); evalState(); px = K/2; py = K/2; pz = 3*K; theta = PI/2; phi = -PI/2; break;
    case '9': L = 9; print("New scale 9..."); K = (1<<L)+1; state = new int[K][K][K]; rLight = new float[K][K][K]; gLight = new float[K][K][K]; bLight = new float[K][K][K]; initState(); evalState(); px = K/2; py = K/2; pz = 3*K; theta = PI/2; phi = -PI/2; break;
    case 'e': saveBig(makeShortRule()+"-"+K+"-"+int(random(10000))+".png"); break;
    case 'y': sideViewSaveIC(); break;
    case 'l': selectInput("Select rule to load:", "fileSelected"); break;
    case ',': lambda -= 0.05; println(lambda); break;
    case '.': lambda += 0.05; println(lambda); break;
    case '[': beta -= 0.05; println(beta); break;
    case ']': beta += 0.05; println(beta); break;
    case '{': mag -= 0.05; println(mag); break;
    case '}': mag += 0.05; println(mag); break;
    case 'r': randomRule(lambda); initState(); evalState(); break;
    case 'i': randomIsingRule(beta,mag); initState(); evalState(); break;
    case ' ': initState(); evalState(); break;
    case 'x': halfState(); evalState(); break;
    case 'c': doubleState(); evalState(); break;
    case 'b': drawBack = !drawBack; break;
    case 'v': drawViewfinder = !drawViewfinder; break;
    case TAB: pressedDir[0] = !pressedDir[0]; break; 
    
    case 'w': case 'W': pressedDir[0] = true; break;
    case 's': case 'S': pressedDir[1] = true; break;
    case 'a': case 'A': pressedDir[2] = true; break;
    case 'd': case 'D': pressedDir[3] = true; break;
    case 'q': case 'Q': pressedDir[4] = true; break;
    case 'z': case 'Z': pressedDir[5] = true; break;
    
    case CODED: if(keyCode == SHIFT) shifted = true; break;
  }
}

void keyReleased() {
  switch(key) {
    case 'w': case 'W': pressedDir[0] = false; break;
    case 's': case 'S': pressedDir[1] = false; break;
    case 'a': case 'A': pressedDir[2] = false; break;
    case 'd': case 'D': pressedDir[3] = false; break;
    case 'q': case 'Q': pressedDir[4] = false; break;
    case 'z': case 'Z': pressedDir[5] = false; break;
    
    case CODED: if(keyCode == SHIFT) shifted = false; break;
  }
}

// load rule from an image.  If it is a SVI, load the initial conditions
void fileSelected(File selection) {
  if (selection != null) {
    String[] splitSelection = split(selection.getName(),"-");
    String ruleString = splitSelection[0];
    readShortRule(ruleString);
    if (splitSelection[splitSelection.length-1].equals("SVI.png")) {
      PImage initData = loadImage(selection.getAbsolutePath());
      initData.loadPixels();
      K = initData.width;
      L = 31 - Integer.numberOfLeadingZeros(K-1);
      state = new int[K][K][K]; 
      for (int i = 0; i < K ; i++) {
        for (int j = 0; j < K; j++) {
          state[i][j][K-1] = initData.pixels[i+j*K]&3;
        }
      }
      initData.updatePixels();
    }
    else {
      initState();
    }
    px = K/2; py = K/2; pz = 3*K; theta = PI/2; phi = -PI/2; 
    evalState();
  }
}

// return the string version of the rule
String makeShortRule() {
  // first make a big number
  BigInteger temp = BigInteger.ZERO;
  for (int i = 0; i < 9; i++) {
    for (int j = 0; j < 9-i; j++) {
      temp = temp.multiply(BigInteger.valueOf(3));
      temp = temp.add(BigInteger.valueOf(cubeRule[i][j]));
    }
  }
  for (int i = 0; i < 7; i++) {
    for (int j = 0; j < 7-i; j++) {
      temp = temp.multiply(BigInteger.valueOf(3));
      temp = temp.add(BigInteger.valueOf(faceRule[i][j]));
    }
  }
  for (int i = 0; i < 7; i++) {
    for (int j = 0; j < 7-i; j++) {
      temp = temp.multiply(BigInteger.valueOf(3));
      temp = temp.add(BigInteger.valueOf(edgeRule[i][j]));
    }
  }
  // then expand in base 62 = 2*26+10
  
  String out = "";
  while (!temp.equals(BigInteger.ZERO)) {
    out += base62(temp.mod(BigInteger.valueOf(62)).intValue());
    temp = temp.divide(BigInteger.valueOf(62));
  }
  return out;
}

// load the rule from a string
void readShortRule(String in) {
  // first make a big number
  BigInteger temp = BigInteger.ZERO;
  for (int i = in.length()-1; i >= 0; i--) {
    temp = temp.multiply(BigInteger.valueOf(62));
    temp = temp.add(BigInteger.valueOf(base62(in.charAt(i))));
  }
  
  // then re-expand into base 3 and use it
  for (int i = 6; i >= 0; i--) {
    for (int j = 6-i; j >= 0; j--) {
      edgeRule[i][j] = temp.mod(BigInteger.valueOf(3)).intValue();
      temp = temp.divide(BigInteger.valueOf(3));
    }
  }
  for (int i = 6; i >= 0; i--) {
    for (int j = 6-i; j >= 0; j--) {
      faceRule[i][j] = temp.mod(BigInteger.valueOf(3)).intValue();
      temp = temp.divide(BigInteger.valueOf(3));
    }
  }
  for (int i = 8; i >= 0; i--) {
    for (int j = 8-i; j >= 0; j--) {
      cubeRule[i][j] = temp.mod(BigInteger.valueOf(3)).intValue();
      temp = temp.divide(BigInteger.valueOf(3));
    }
  }
}

// turn an int into my chosen base 62 version
char base62(int in) {
  if (in < 10) return char(in+48);
  if (in < 36) return char((in-10)+97);
  return char((in-36)+65);
}

// turn a char into my chosen base 62 version
int base62(char in) {
  if (in < 58) return int(in) - int('0');
  if (in < 91) return int(in) - int('A') + 36;
  return int(in) - int('a') + 10;
}

// camera state
float px = K/2;
float py = K/2;
float pz = 3*K;
float theta = PI/2;
float phi = -PI/2;

// hide the pointer and lock to the center of the window when clicked
void mousePressed() {
  GLWindow r = (GLWindow)surface.getNative();
  r.warpPointer((width*displayDensity())/2,(height*displayDensity())/2);
  r.confinePointer(true);
  noCursor();  
}

// show and release the pointer when released
void mouseReleased() {
  GLWindow r = (GLWindow)surface.getNative();
  r.confinePointer(false);
  cursor();  
}

// rotate camera when dragged
void mouseDragged() {
  GLWindow r = (GLWindow)surface.getNative();
  float dx = (mouseX-width/2)/200.0; // PIXEL DENSITY IS CONFUSING
  float dy = (mouseY-height/2)/200.0;
  theta = constrain(theta+dy,0.001,PI-0.001);
  phi = phi-dx;
  r.warpPointer((width*displayDensity())/2,(height*displayDensity())/2);
}

// compute the sign of a float
int sgn(float in) {
  return in>=0?1:-1;  
}

// draw the scene
void draw() {
  float dx = sin(theta)*cos(phi);
  float dz = sin(theta)*sin(phi);
  float dy = cos(theta);
  
  float speed = K/8;
  if (shifted) {
    speed *= 3; 
  }
  
  if (pressedDir[0]) {px += speed*dx/frameRate; py += speed*dy/frameRate; pz += speed*dz/frameRate;}
  if (pressedDir[1]) {px -= speed*dx/frameRate; py -= speed*dy/frameRate; pz -= speed*dz/frameRate;}
  if (pressedDir[2]) {px += speed*dz/(sqrt(dx*dx+dz*dz)*frameRate); pz -= speed*dx/(sqrt(dx*dx+dz*dz)*frameRate);}
  if (pressedDir[3]) {px -= speed*dz/(sqrt(dx*dx+dz*dz)*frameRate); pz += speed*dx/(sqrt(dx*dx+dz*dz)*frameRate);}
  if (pressedDir[4]) {py -= speed/frameRate;}
  if (pressedDir[5]) {py += speed/frameRate;}

  camera(px,py,pz,px+dx,py+dy,pz+dz,0,1,0);
  perspective(PI/6, (1.0*width)/height, 0.1, 9*K);
  
  if (drawBack) {
    pushMatrix();
    translate(px,py,pz);
    shape(skySphere);
    popMatrix();
  }
  else background(102,153,204);
  
  shader(smoothShader);
  
  beginShape(QUADS);
  noStroke();
  texture(bot);
  vertex(0,K,0,0.5,0.5);
  vertex(K,K,0,K-0.5,0.5);
  vertex(K,K,K,K-0.5,K-0.5);
  vertex(0,K,K,0.5,K-0.5);
  endShape();
  
  int dir = 0;
  if (abs(dz) > abs(dx)) dir = 1;
  if (abs(dy) > max(abs(dx),abs(dz))) dir = 2;

  int tx = int(sqrt(K));
  int layers = 1;
  
  if (dir == 0) {
    beginShape(QUADS);
    noStroke();
    texture(texX);
    
    int stepX = -sgn(dx);
    int initX = stepX==1?0:(K-1);
    int finlX = stepX==1?K:-1;
    
    int initL = stepX==1?0:(layers-1);
    int finlL = stepX==1?layers:-1;
    
    for (int i = initX; i != finlX; i += stepX) {
      for (int l = initL; l != finlL; l += stepX) {
        int ix = i%tx;
        int iy = i/tx;
        vertex(i+(1.0+2*l)/(2*layers),0,0,ix*(K+1)+0.5,iy*(K+1)+0.5);
        vertex(i+(1.0+2*l)/(2*layers),K,0,ix*(K+1)+0.5,iy*(K+1)+K-0.5);
        vertex(i+(1.0+2*l)/(2*layers),K,K,ix*(K+1)+K-0.5,iy*(K+1)+K-0.5);
        vertex(i+(1.0+2*l)/(2*layers),0,K,ix*(K+1)+K-0.5,iy*(K+1)+0.5);
      }
    }
    endShape();
  }
  
  if (dir == 1) {
    beginShape(QUADS);
    noStroke();
    texture(texY);
    
    int stepZ = -sgn(dz);
    int initZ = stepZ==1?0:(K-1);
    int finlZ = stepZ==1?K:-1;
    
    int initL = stepZ==1?0:(layers-1);
    int finlL = stepZ==1?layers:-1;
    
    for (int i = initZ; i != finlZ; i += stepZ) {
      for (int l = initL; l != finlL; l += stepZ) {
        int ix = i%tx;
        int iy = i/tx;
        vertex(0,0,i+(1.0+2*l)/(2*layers),ix*(K+1)+0.5,iy*(K+1)+0.5);
        vertex(K,0,i+(1.0+2*l)/(2*layers),ix*(K+1)+K-0.5,iy*(K+1)+0.5);
        vertex(K,K,i+(1.0+2*l)/(2*layers),ix*(K+1)+K-0.5,iy*(K+1)+K-0.5);
        vertex(0,K,i+(1.0+2*l)/(2*layers),ix*(K+1)+0.5,iy*(K+1)+K-0.5);
      }
    }
    endShape();
  }
  
  if (dir == 2) {
    beginShape(QUADS);
    noStroke();
    texture(texZ);
    
    int stepY = -sgn(dy);
    int initY = stepY==1?0:(K-1);
    int finlY = stepY==1?K:-1;
    
    int initL = stepY==1?0:(layers-1);
    int finlL = stepY==1?layers:-1;
    
    for (int i = initY; i != finlY; i += stepY) {
      for (int l = initL; l != finlL; l += stepY) {
        int ix = i%tx;
        int iy = i/tx;
        vertex(0,i+(1.0+2*l)/(2*layers),0,ix*(K+1)+0.5,iy*(K+1)+0.5);
        vertex(K,i+(1.0+2*l)/(2*layers),0,ix*(K+1)+K-0.5,iy*(K+1)+0.5);
        vertex(K,i+(1.0+2*l)/(2*layers),K,ix*(K+1)+K-0.5,iy*(K+1)+K-0.5);
        vertex(0,i+(1.0+2*l)/(2*layers),K,ix*(K+1)+0.5,iy*(K+1)+K-0.5);
      }
    }
    endShape();
  }
  
  if (py > K) {
    beginShape(QUADS);
    noStroke();
    texture(bot);
    vertex(0,K,0,0.5,0.5);
    vertex(K,K,0,K-0.5,0.5);
    vertex(K,K,K,K-0.5,K-0.5);
    vertex(0,K,K,0.5,K-0.5);
    endShape();
  }
  
  camera();
  perspective();
  
  if (drawViewfinder) {
    stroke(255,64);
    noFill();
    rect(129,41,width-258,height-82);
    rect(41,129,width-82,height-258);
    
    stroke(255,255);
    noFill();
    
    cross(129,41);
    cross(41,129);
    cross(129,129);
    
    cross(355,41);
    cross(443,129);
    cross(355,129);
    
    cross(129,443);
    cross(41,355);
    cross(129,355);
    
    cross(355,443);
    cross(443,355);
    cross(355,355);
  }
}

// draw a cross for the optional viewfinder
void cross(int i, int j) {
  int s = 2;
  line(i-s-1,j,i+s,j);
  line(i,j-s,i,j+s+1);
}

// save a large version of the image
void saveBig(String name) {
  PGraphics out = createGraphics(2662,2662,P3D);
  
  out.beginDraw();
  out.hint(DISABLE_DEPTH_TEST);
  out.hint(DISABLE_TEXTURE_MIPMAPS);
  
  float dx = sin(theta)*cos(phi);
  float dz = sin(theta)*sin(phi);
  float dy = cos(theta);

  out.camera(px,py,pz,px+dx,py+dy,pz+dz,0,1,0);
  out.perspective(PI/6, (1.0*width)/height, 0.1, 9*K);
  
  if (drawBack) {
    out.pushMatrix();
    out.translate(px,py,pz);
    out.shape(skySphere);
    out.popMatrix();
  }
  else out.background(102,153,204);
  
  out.shader(smoothShader);
  
  out.beginShape(QUADS);
  out.noStroke();
  out.texture(bot);
  out.vertex(0,K,0,0.5,0.5);
  out.vertex(K,K,0,K-0.5,0.5);
  out.vertex(K,K,K,K-0.5,K-0.5);
  out.vertex(0,K,K,0.5,K-0.5);
  out.endShape();
  
  int dir = 0;
  if (abs(dz) > abs(dx)) dir = 1;
  if (abs(dy) > max(abs(dx),abs(dz))) dir = 2;

  int tx = int(sqrt(K));
  int layers = 1;
  
  if (dir == 0) {
    out.beginShape(QUADS);
    out.noStroke();
    out.texture(texX);
    
    int stepX = -sgn(dx);
    int initX = stepX==1?0:(K-1);
    int finlX = stepX==1?K:-1;
    
    int initL = stepX==1?0:(layers-1);
    int finlL = stepX==1?layers:-1;
    
    for (int i = initX; i != finlX; i += stepX) {
      for (int l = initL; l != finlL; l += stepX) {
        int ix = i%tx;
        int iy = i/tx;
        out.vertex(i+(1.0+2*l)/(2*layers),0,0,ix*(K+1)+0.5,iy*(K+1)+0.5);
        out.vertex(i+(1.0+2*l)/(2*layers),K,0,ix*(K+1)+0.5,iy*(K+1)+K-0.5);
        out.vertex(i+(1.0+2*l)/(2*layers),K,K,ix*(K+1)+K-0.5,iy*(K+1)+K-0.5);
        out.vertex(i+(1.0+2*l)/(2*layers),0,K,ix*(K+1)+K-0.5,iy*(K+1)+0.5);
      }
    }
    out.endShape();
  }
  
  if (dir == 1) {
    out.beginShape(QUADS);
    out.noStroke();
    out.texture(texY);
    
    int stepZ = -sgn(dz);
    int initZ = stepZ==1?0:(K-1);
    int finlZ = stepZ==1?K:-1;
    
    int initL = stepZ==1?0:(layers-1);
    int finlL = stepZ==1?layers:-1;
    
    for (int i = initZ; i != finlZ; i += stepZ) {
      for (int l = initL; l != finlL; l += stepZ) {
        int ix = i%tx;
        int iy = i/tx;
        out.vertex(0,0,i+(1.0+2*l)/(2*layers),ix*(K+1)+0.5,iy*(K+1)+0.5);
        out.vertex(K,0,i+(1.0+2*l)/(2*layers),ix*(K+1)+K-0.5,iy*(K+1)+0.5);
        out.vertex(K,K,i+(1.0+2*l)/(2*layers),ix*(K+1)+K-0.5,iy*(K+1)+K-0.5);
        out.vertex(0,K,i+(1.0+2*l)/(2*layers),ix*(K+1)+0.5,iy*(K+1)+K-0.5);
      }
    }
    out.endShape();
  }
  
  if (dir == 2) {
    out.beginShape(QUADS);
    out.noStroke();
    out.texture(texZ);
    
    int stepY = -sgn(dy);
    int initY = stepY==1?0:(K-1);
    int finlY = stepY==1?K:-1;
    
    int initL = stepY==1?0:(layers-1);
    int finlL = stepY==1?layers:-1;
    
    for (int i = initY; i != finlY; i += stepY) {
      for (int l = initL; l != finlL; l += stepY) {
        int ix = i%tx;
        int iy = i/tx;
        out.vertex(0,i+(1.0+2*l)/(2*layers),0,ix*(K+1)+0.5,iy*(K+1)+0.5);
        out.vertex(K,i+(1.0+2*l)/(2*layers),0,ix*(K+1)+K-0.5,iy*(K+1)+0.5);
        out.vertex(K,i+(1.0+2*l)/(2*layers),K,ix*(K+1)+K-0.5,iy*(K+1)+K-0.5);
        out.vertex(0,i+(1.0+2*l)/(2*layers),K,ix*(K+1)+0.5,iy*(K+1)+K-0.5);
      }
    }
    out.endShape();
  }
  
  if (py > K) {
    out.beginShape(QUADS);
    out.noStroke();
    out.texture(bot);
    out.vertex(0,K,0,0.5,0.5);
    out.vertex(K,K,0,K-0.5,0.5);
    out.vertex(K,K,K,K-0.5,K-0.5);
    out.vertex(0,K,K,0.5,K-0.5);
    out.endShape();
  }

  out.endDraw();
  out.save(name);
}

// the textures needed to display
PImage texX;
PImage texY;
PImage texZ;
PImage bot;

// turn the universe into large texture that packs all the slices
void makeTexes() {
  int tx = int(sqrt(K));
  int ty = K/tx+1;
  
  color[][][] colored = new color[K][K][K];
  for (int i = 0; i < K; i++) {
    for (int j = 0; j < K; j++) {
      for (int k = 0; k < K; k++) {
        if (state[i][j][k] != 0) colored[i][j][k] = color(255*rLight[i][j][k],255*gLight[i][j][k],255*bLight[i][j][k]);
        if (state[i][j][k] == 0) {
          // bleeding the color into adjacent transparent cells prevents odd halo effects
          float r = 0;
          float g = 0;
          float b = 0;
          int c = 0;
          for (int di = -1; di <= 1; di++) {
            for (int dj = -1; dj <= 1; dj++) {
              for (int dk = -1; dk <= 1; dk++) {
                if ((i+di >= 0) && (i+di < K) && (j+dj >= 0) && (j+dj < K) && (k+dk >= 0) && (k+dk < K)) {
                  if (state[i+di][j+dj][k+dk] != 0) {
                    r += 255*rLight[i+di][j+dj][k+dk];
                    g += 255*gLight[i+di][j+dj][k+dk];
                    b += 255*bLight[i+di][j+dj][k+dk];
                    c++;
                  }
                }
              }
            }
          }

          if (c != 0) colored[i][j][k] = color(r/c,g/c,b/c,0);
          else colored[i][j][k] = color(255,0);
        }
      }
    }
  }
  
  texX = createImage((K+1)*tx,(K+1)*ty,ARGB);
  texY = createImage((K+1)*tx,(K+1)*ty,ARGB);
  texZ = createImage((K+1)*tx,(K+1)*ty,ARGB);
  
  texX.loadPixels();
  texY.loadPixels();
  texZ.loadPixels();
  for (int i = 0; i < K; i++) {
    int ix = i%tx;
    int iy = i/tx;
    for (int j = 0; j < K; j++) {
      for (int k = 0; k < K; k++) {
        texX.pixels[(ix*(K+1) + (iy*(K+1))*(K+1)*tx)+(j+(K+1)*tx*k)] = colored[i][j][k];
        texY.pixels[(ix*(K+1) + (iy*(K+1))*(K+1)*tx)+(j+(K+1)*tx*k)] = colored[j][i][k];
        texZ.pixels[(ix*(K+1) + (iy*(K+1))*(K+1)*tx)+(j+(K+1)*tx*k)] = colored[j][k][i];
      }
    }
  }
  texX.updatePixels();
  texY.updatePixels();
  texZ.updatePixels();
  
  bot = createImage(K,K,ARGB);
  bot.loadPixels();
  for (int i = 0; i < K; i++) {
    for (int j = 0; j < K; j++) {
      bot.pixels[i+K*j] = colored[i][j][K-1];
    }
  }
  bot.updatePixels();
}

// save a PNG image with the side view and initial conditions hidden in the least significant bits (SVI = Side View with Initial conditions)
void sideViewSaveIC() {
  PImage out = createImage(K,K,ARGB);  
  out.loadPixels();
  for (int i = 0; i < K; i++) {
    for (int j = 0; j < K; j++) {
      boolean found = false;
      for (int k = K-1; k >= 0; k--) {
        if (state[i][k][j] != 0) {
          out.pixels[i+K*j] = color(255*rLight[i][k][j],255*gLight[i][k][j],255*bLight[i][k][j]);
          
          found = true;
          break;
        }
      }
      if (!found) {
        out.pixels[i+K*j] = skyGradient.get(0, int((skyGradient.height*(2.0/3.0)*j)/K) );
      }
      out.pixels[i+K*j] = ((out.pixels[i+K*j]>>2)<<2) + state[i][j][K-1];
    }
  }
  out.updatePixels();
  out.save(makeShortRule()+"-"+K+"-"+int(random(10000))+"-SVI.png");
}