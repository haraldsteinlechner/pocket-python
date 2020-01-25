import cv2 
import sys

def appendName(filename):
    return "{0}_{2}.{1}".format(*filename.rsplit('.', 1) + ["_flip"])
	
def flip(image_path):

	flip_image_path = appendName(image_path)

	im = cv2.imread(image_path)
	im_flip = cv2.flip(im, 0)
	cv2.imwrite(flip_image_path, im_flip)
	
	print(flip_image_path)
	return flip_image_path
	
flip(sys.argv[1])