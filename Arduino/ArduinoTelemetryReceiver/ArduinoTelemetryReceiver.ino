#include <Stepper.h>
#include <Wire.h>
#include <Adafruit_MCP23017.h>
#include <Adafruit_GFX.h>
#include <Adafruit_ILI9341.h>

#define BACKLIGHT_POTENTIOMETER A8
#define BACKLIGHT_MIN_DUTY 10
#define BACKLIGHT_MAX_DUTY 128
#define BLINK_LENGTH 600

#define BACKLIGHT_WB 12
#define BACKLIGHT_WS 11
#define BACKLIGHT_RB 10
#define BACKLIGHT_RS 9
#define BACKLIGHT_LCD 8

#define MOTOR_LATENCY 4
#define MOTOR_MAX_STEP 1
#define MOTOR_BIG_STEPS 600
#define MOTOR_BIG_SPEED 60
#define MOTOR_SMALL_STEPS 210
#define MOTOR_SMALL_SPEED 180

#define MOTOR_A_1 38
#define MOTOR_B_1 34
#define MOTOR_C_1 30
#define MOTOR_D_1 26

#define TFT_CS 48
#define TFT_DC 49

String serialBuffer = "";
boolean serialDataIsReady = false;

long lastBlinkStateSwitch = 0;
bool ledBlinkState = false;
String ledState = "0000000000000000";
String backlightState = "00000";
int currentBacklightDuty = 0;

Adafruit_ILI9341 tft = Adafruit_ILI9341(TFT_CS, TFT_DC);
String regA[5];
String regB[5];
String regC[5];
bool regAchanged = false;
bool regBchanged = false;
bool regCchanged = false;
bool clearValuesRequired = false;
int screenId = 0;

Adafruit_MCP23017 ledController;
Stepper gaugeMotorA(MOTOR_BIG_STEPS, MOTOR_A_1, MOTOR_A_1 + 1, MOTOR_A_1 + 2, MOTOR_A_1 + 3);
Stepper gaugeMotorB(MOTOR_SMALL_STEPS, MOTOR_B_1, MOTOR_B_1 + 1, MOTOR_B_1 + 2, MOTOR_B_1 + 3);
Stepper gaugeMotorC(MOTOR_SMALL_STEPS, MOTOR_C_1, MOTOR_C_1 + 1, MOTOR_C_1 + 2, MOTOR_C_1 + 3);
Stepper gaugeMotorD(MOTOR_BIG_STEPS, MOTOR_D_1, MOTOR_D_1 + 1, MOTOR_D_1 + 2, MOTOR_D_1 + 3);
int gaugeCurrentStep[4];
int gaugeTargetStep[4];

void splitData(String input, int count, String *output) {
  int cursorPosition = 0;
  int idx = 0;
  for(int i = 0; i < input.length(); i++) {
    if(input.substring(i, i + 1) == "_") {
      output[idx] = input.substring(cursorPosition, i);
      cursorPosition = i + 1;
      idx++;
      if(idx == count - 1) break;
    }
  }
  output[count - 1] = input.substring(cursorPosition, input.length());
}

void updateLeds(String state) {
  if(state.length() == 16) {
    ledState = state;
    for(int i = 0; i < 16; i++) {
      String val = state.substring(i, i+1);
      if(val == "2") {
        ledController.digitalWrite(i, ledBlinkState ? HIGH : LOW);
      }else{
        ledController.digitalWrite(i, val == "1" ? HIGH : LOW);
      }
    }
  }
}

void updateBlinkingLeds() {
  if(millis() - lastBlinkStateSwitch >= BLINK_LENGTH) {
    lastBlinkStateSwitch = millis();
    ledBlinkState = !ledBlinkState;
    for(int i = 0; i < 16; i++) {
      if(ledState.substring(i, i+1) == "2") ledController.digitalWrite(i, ledBlinkState ? HIGH : LOW);
    }
  }
}

void updateBacklights(String state){
  if(state.length() == 5) {
    backlightState = state;
    int blPower = map(analogRead(BACKLIGHT_POTENTIOMETER), 0, 1023, BACKLIGHT_MIN_DUTY, BACKLIGHT_MAX_DUTY);
    analogWrite(BACKLIGHT_WB, state.substring(0, 1) == "1" ? blPower : 0);
    analogWrite(BACKLIGHT_WS, state.substring(1, 2) == "1" ? blPower : 0);
    digitalWrite(BACKLIGHT_RB, state.substring(2, 3) == "1" ? HIGH : LOW);
    digitalWrite(BACKLIGHT_RS, state.substring(3, 4) == "1" ? HIGH : LOW);
    digitalWrite(BACKLIGHT_LCD, state.substring(4, 5) == "0" ? HIGH : LOW);
  }
}

void adjustBacklights() {
  int blPower = map(analogRead(BACKLIGHT_POTENTIOMETER), 0, 1023, BACKLIGHT_MIN_DUTY, BACKLIGHT_MAX_DUTY);
  if(currentBacklightDuty != blPower) {
    currentBacklightDuty = blPower;
    analogWrite(BACKLIGHT_WB, backlightState.substring(0, 1) == "1" ? blPower : 0);
    analogWrite(BACKLIGHT_WS, backlightState.substring(1, 2) == "1" ? blPower : 0);
  }
}

void updateGauges(String state) {
  String values[4];
  splitData(state, 4, values);
  for(int i = 0; i < 4; i++) {
    gaugeTargetStep[i] = values[i].toFloat() * (i == 0 || i == 3 ? MOTOR_BIG_STEPS : MOTOR_SMALL_STEPS) * 0.01;
  }
}

void moveMotors() {
  for(int i = 0; i < 4; i++) {
    int diff = gaugeCurrentStep[i] - gaugeTargetStep[i];
    if(abs(diff) >= MOTOR_LATENCY) {
      diff = min(abs(diff), MOTOR_MAX_STEP) * (diff < 0 ? -1 : 1);
      switch(i) {
        case 0: gaugeMotorA.step(diff); break;
        case 1: gaugeMotorB.step(diff); break;
        case 2: gaugeMotorC.step(diff); break;
        case 3: gaugeMotorD.step(diff); break;
      }
      gaugeCurrentStep[i] = gaugeCurrentStep[i] - diff;
    }
  }
}

void resetLeds() {
  updateLeds("0000000000000000");
}

void resetBacklights() {
  updateBacklights("00000");
}

void resetGauges() {
  updateGauges("0_0_0_0");
}

void updateRegistry(int id, String data) {
  String v[5];
  splitData(data, 5, v);
  switch(id) {
    case 1:
      for(int i = 0; i < 5; i++) regA[i] = v[i];
      regAchanged = true;
      break;
    case 2:
      for(int i = 0; i < 5; i++) regB[i] = v[i];
      regBchanged = true;
      break;
    case 3:
      for(int i = 0; i < 5; i++) regC[i] = v[i];
      regCchanged = true;
      break;
  }
}

void clearLcd() {
  tft.fillScreen(ILI9341_BLACK);
}

void printTopBar() {
  int w = tft.width(), h = tft.height();
  tft.fillRect(5, 2, w - 5, 44, ILI9341_BLACK);
  //GEAR
  tft.drawRect(w - 60, 4, 55, 40, ILI9341_WHITE);
  tft.setCursor(w - 50, 13);
  tft.setTextSize(3);
  tft.println(regA[0]);
  //ECO SHIFT
  if(regA[1] == "UP") tft.fillTriangle(w - 80, 12, w - 70, 35, w - 90, 35, ILI9341_GREEN);
  if(regA[1] == "DN") tft.fillTriangle(w - 90, 12, w - 70, 12, w - 80, 35, ILI9341_RED);
  //CLOCK
  tft.setCursor(12, 4);
  tft.setTextSize(2);
  tft.println(regA[2]);
  //CC SPEED
  if(regA[3] != "0") {
    tft.fillRect(14, 23, 30, 16, ILI9341_WHITE);
    tft.setCursor(18, 24);
    tft.setTextColor(ILI9341_BLACK);
    tft.println("CC");
    tft.setCursor(50, 24);
    tft.setTextColor(ILI9341_WHITE);
    tft.println(regA[3] + "km/h");
  }
}

void printBottomBar() {
  int w = tft.width(), h = tft.height();
  tft.fillRect(5, h - 80, w - 5, 72, ILI9341_BLACK);
  //SEPARATOR
  tft.drawLine(11, h - 76, w - 6, h - 76, ILI9341_WHITE);
  if(regC[0] == "0") {
    //RETARDER
    if(regC[2] != "0") {
      tft.setCursor(16, h - 62);
      tft.println("R");
      for(int i = 0; i < regC[2].toInt(); i++) {
        if(i < regC[1].toInt()) {
          tft.fillRect(34 + i * 16, h - 65, 12, 20, ILI9341_WHITE);
        }else{
          tft.drawRect(34 + i * 16, h - 65, 12, 20, ILI9341_WHITE);
        }
      }
    }
    //ODOMETER
    tft.setCursor(12, h - 34);
    tft.println(regC[3] + "km");
    //SPEED LIMIT
    if(regC[4] != "0") {
      tft.fillCircle(w - 32, h - 40, 26, ILI9341_RED);
      tft.fillCircle(w - 32, h - 40, 22, ILI9341_WHITE);
      tft.setCursor(w - 43, h - 47);
      tft.setTextColor(ILI9341_BLACK);
      tft.println(regC[4]);
      tft.setTextColor(ILI9341_WHITE);
    }
  }else{
    //ALERTS
    int id = regC[0].toInt();
    uint16_t alertBg = ILI9341_CYAN;
    uint16_t alertFg = ILI9341_BLACK;
    String alertText1 = "";
    String alertText2 = "";
    if(id >= 10 && id < 20) {
      alertBg = ILI9341_ORANGE;
      alertFg = ILI9341_BLACK;
    }else if(id >= 30) {
      alertBg = ILI9341_RED;
      alertFg = ILI9341_WHITE;
    }
    switch(id) {
      case 31: alertText1 = "Brake low"; alertText2 = "pressure!"; break;
    }
    tft.fillRect(11, h - 70, w - 16, 55, alertBg);
    tft.setTextColor(alertFg);
    tft.setCursor(17, h - 64);
    tft.println(alertText1);
    tft.setCursor(17, h - 38);
    tft.println(alertText2);
  }
}

void printLcd(int id) {
  screenId = id;
  clearLcd();
  int w = tft.width(), h = tft.height();
  if(id > 2) {
    printTopBar();
    tft.drawLine(11, 50, w - 6, 50, ILI9341_WHITE);
  }
  if(id > 2 && id < 10) printBottomBar();
  switch(id) {
    case 1: //TESTING
      tft.fillScreen(regB[0] == "0" ? ILI9341_RED : regB[0] == "1" ? ILI9341_GREEN : regB[0] == "2" ? ILI9341_BLUE : ILI9341_WHITE);
      break;
    case 2: //INITIAL IMAGE
      tft.setCursor(40, 40);
      tft.setTextSize(5);
      tft.setTextColor(ILI9341_WHITE);
      tft.println("HELLO");
      break;
  }
  clearValuesRequired = false;
}

void updateLcd() {
  int w = tft.width(), h = tft.height();
  switch(screenId) {
    case 1: //TESTING
      if(clearValuesRequired) clearLcd();
      break;
    case 2: //INITIAL IMAGE

      break;
  }
  clearValuesRequired = true;
}

void setup() {
  Serial.begin(9600);
  //LCD
  tft.begin();
  tft.setRotation(0);
  //LED CONTROLLER
  ledController.begin();
  for(int i = 0; i < 16; i++) ledController.pinMode(i, OUTPUT);
  resetLeds();
  //BACKLIGHT
  pinMode(BACKLIGHT_WB, OUTPUT);
  pinMode(BACKLIGHT_WS, OUTPUT);
  pinMode(BACKLIGHT_RB, OUTPUT);
  pinMode(BACKLIGHT_RS, OUTPUT);
  pinMode(BACKLIGHT_LCD, OUTPUT);
  resetBacklights();
  //GAUGE
  gaugeMotorA.setSpeed(MOTOR_BIG_SPEED);
  gaugeMotorB.setSpeed(MOTOR_SMALL_SPEED);
  gaugeMotorC.setSpeed(MOTOR_SMALL_SPEED);
  gaugeMotorD.setSpeed(MOTOR_BIG_SPEED);
  resetGauges();

  updateBacklights("00001");
  updateRegistry(1, "11_DN_17:03_80_");
  updateRegistry(3, "0_2_5_180000_80");
  printLcd(3);
}

void loop() {
  if(serialDataIsReady) {
    String cmdType = serialBuffer.substring(0, 3);
    String cmdData = serialBuffer.substring(4, serialBuffer.length());
    if(cmdType == "LED"){
      updateLeds(cmdData);
    }else if(cmdType == "BKL") {
      updateBacklights(cmdData);
    }else if(cmdType == "GAU") {
      updateGauges(cmdData);
    }else if(cmdType == "REA") {
      updateRegistry(1, cmdData);
    }else if(cmdType == "REB") {
      updateRegistry(2, cmdData);
    }else if(cmdType == "REC") {
      updateRegistry(3, cmdData);
    }else if(cmdType == "PRT") {
      printLcd(cmdData.toInt());
    }else if(cmdType == "UPD") {
      updateLcd();
    }
    serialDataIsReady = false;
    serialBuffer = "";
  }
  updateBlinkingLeds();
  adjustBacklights();
  moveMotors();
}

void serialEvent() {
  while(Serial.available() > 0) {
    char inChar = (char)Serial.read();
    if(inChar == '\n') {
      serialDataIsReady = true;
      break;
    }else{
      serialBuffer += inChar;
    }
  }
}
