open System
open System.Net
open System.IO
open System.IO.Compression

open System
open System.Diagnostics
open System.Text
open System.Collections.Generic

open PocketPython


[<EntryPoint;STAThread>]
let main argv = 
    
    let run = Pocket.runIn (printfn "%s") System.Environment.CurrentDirectory ["opencv-python";]

    let img = @"C:\Users\hs\lens\IMG_6976.JPG"

    let result = 
        run """
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
        """ img ""

    if result.exitCode <> 0 then
        failwithf "failed with: %A" result.stderr
    else printfn "flip image is here: %s" (Array.last result.stdout)

    0

