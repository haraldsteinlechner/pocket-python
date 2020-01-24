import lensfunpy
import cv2 
from exif import Image

def readExif(photo):
	f = open(photo, 'rb')
	img = Image(f)
	return img


aperture = 1.4
distance = 10
image_path = 'IMG_6976.JPG'
undistorted_image_path = 'IMG_6976_undistorted.JPG'

#image_path = 'IMG_6981.JPG'
#undistorted_image_path = 'IMG_6981.JPG_undistorted.JPG'

img = readExif(image_path)
#print(dir(img))

cam_maker = img["make"] 
cam_model = img["model"] 

#print(cam_maker)
#print(cam_model)

focal_length = img["focal_length"]
aperture = img["aperture_value"]

db = lensfunpy.Database()
cam = db.find_cameras(cam_maker, cam_model)[0]
lens = db.find_lenses(cam)[0]

print(cam)
print(lens)
print(focal_length);

im = cv2.imread(image_path)
height, width = im.shape[0], im.shape[1]

mod = lensfunpy.Modifier(lens, cam.crop_factor, width, height)
mod.initialize(focal_length, aperture, distance)

undist_coords = mod.apply_geometry_distortion()
im_undistorted = cv2.remap(im, undist_coords, None, cv2.INTER_LANCZOS4)
cv2.imwrite(undistorted_image_path, im_undistorted)