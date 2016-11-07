SS Bump Map Generator v1.0 by Patapom (2014-10-15)

This generator builds a Self-Shadowed Bump map from a height map (http://n00body.squarespace.com/journal/2010/2/7/self-shadowed-bump-maps.html).
Basically what this generator is doing is casting a lot of rays to compute a directional Ambient Occlusion factor for 3 principal directions
 and each AO factor is then stored in the RGB channels. Also, the global AO factor (i.e. non-directional) is stored in the alpha channel.

When launching the UI version:
	1) Click the left pane or drag'n drop an image to load the main height map
	2) Modify the parameters
	3) Click the "Generate" button
	4) Click the right pane to save the result
	
Here is the description of the various parameters:
	• Encoded Height 		=> Maximum displacement value encoded by the height map (i.e. white will be mapped to that value)
	• Physical Texture Size	=> Size of the texture in the world, in centimeters
	• Rays per pixel 		=> Amount of AO rays cast for each pixel
	• Max steps count 		=> Maximum distance for which the rays are traced, if no hit occurs after this then we consider the ray escaped the surface
	• Bilateral Radius 		=> Radius of the bilateral filtering that is performed prior computing AO (bilateral filtering smoothes the results a bit)
	• Bilateral Tolerance 	=> Tolerance value for heights to be merged together (i.e. blurred)
