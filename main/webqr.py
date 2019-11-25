from __future__ import print_function
from waitress import serve
from flask import Flask, send_file, send_from_directory, request
import cv2, qrcode
import numpy as np
import pyzbar.pyzbar as pyzbar
import RPi.GPIO as GPIO
import serial
import time


GPIO.setmode(GPIO.BCM)
GPIO.setup(17, GPIO.OUT)
GPIO.setup(22, GPIO.OUT)
GPIO.setup(27, GPIO.OUT)


app = Flask(__name__)

#For testing av tilkobling
@app.route('/test')
def test():
    print('TEST')
    return "Test received"

@app.route('/')
def SendImage():
    return send_file('test.jpg',mimetype='image/jpg')

#Kalles av circuitpy koden
@app.route('/blueopen')
def openBt():
    GPIO.output(27, GPIO.HIGH)
    print("open:bt")
    return "signal received from bt"

#Kalles av circuitpy koden
@app.route('/blueclose')
def closeBt():
    GPIO.output(27, GPIO.LOW)
    print("close:bt")
    return "signal received from bt"

#Kalles av grensesnittet 
@app.route('/image', methods = ['GET'])
def camImage():
    blink = request.args.get("blink")
    cam = cv2.VideoCapture(0)
    b,img = cam.read()
    #LED 17 skal blink dersom grensesnittet mottar bilder fra webcamera
    cam.release() 
    if b:
        if (blink == "True"):
            GPIO.output(17, GPIO.HIGH)
            #print ("HIGH")
        else:
            GPIO.output(17, GPIO.LOW)
            #print ("LOW")

        cv2.imwrite('Picture.jpg',img)
        img = cv2.resize(img,(320,320))
        decodeObjects = pyzbar.decode(img)
        #Leser data fra webkamera
        for obj in decodeObjects:
            print(obj.data)
            #Dersom QR verdien finnes i keys.txt "åpnes" låsen
            with open ('keys.txt') as f:
                if (obj.data in f.read()):
                    print('unlocked!')
                    #oppdatere LCD
                    displayState("Open")
                    #oppdaterer led
                    GPIO.output(27, GPIO.HIGH)
                else:
                    print('locked!')
                    #oppdatere LCD
                    displayState("Locked")
                    #oppdaterer led
                    GPIO.output(27, GPIO.LOW)
        return send_file('Picture.jpg',mimetype='image/jpg')
    else:
        return "Camera error"

@app.route('/createQR', methods = ['GET'])
def createQR():
    print("Creating QR..")
    textToConvertToQRCode=request.args.get("text")
    with open('keys.txt') as f:
        #Ser etter nøkkelkoden sendt fra grensesnittet i keys.txt fila
        #Genererer QR kode dersom koden finnes i keys.txt
        if (textToConvertToQRCode in f.read()):
            f.close()
            qr = qrcode.QRCode(version=1, box_size=15, border=5)
            qr.add_data(textToConvertToQRCode)
            qr.make(fit=True)
            img = qr.make_image(fill='black', back_color='white')
            img.save('qrCode.png')
            return send_file('qrCode.png', mimetype='image/png')
        else:
            f.close()
            return send_from_directory("/home/pi/opg", "no.png", mimetype='image/png')
#Viser tilstand på LCD
def displayState(state):
    port = "/dev/ttyACM0"
    SerialIOmbed = serial.Serial(port,9600)
    SerialIOmbed.flushInput()
    time.sleep(2)
    SerialIOmbed.write(state)
    SerialIOmbed.write("\n")
    SerialIOmbed.close()

serve(app,host='0.0.0.0',port=5000)
