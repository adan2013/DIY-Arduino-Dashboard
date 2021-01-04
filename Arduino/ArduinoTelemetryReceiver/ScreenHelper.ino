void clearLcd() {
  tft.fillScreen(ILI9341_BLACK);
}

String getAlertText(int id, bool firstLine) {
  switch(id) {
    case 31: return firstLine ? "Brake low" : "pressure!";
    default: return "";
  }
}

void printTopBar() {
  int w = tft.width(), h = tft.height();
  tft.fillRect(5, 2, w - 5, 44, ILI9341_BLACK);
  //GEAR
  tft.drawRect(w - 60, 4, 55, 40, ILI9341_WHITE);
  tft.setCursor(w - (regA[0].length() < 2 ? 40 : 50), 13);
  tft.setTextSize(3);
  tft.println(regA[0]);
  //ECO SHIFT
  if(regA[1] == "UP") tft.fillTriangle(w - 80, 10, w - 70, 21, w - 90, 21, ILI9341_GREEN);
  if(regA[1] == "DN") tft.fillTriangle(w - 90, 25, w - 70, 25, w - 80, 36, ILI9341_RED);
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
    if(id >= 10 && id < 20) {
      alertBg = ILI9341_ORANGE;
      alertFg = ILI9341_BLACK;
    }else if(id >= 30) {
      alertBg = ILI9341_RED;
      alertFg = ILI9341_WHITE;
    }
    tft.fillRoundRect(11, h - 70, w - 16, 55, 8, alertBg);
    tft.setTextColor(alertFg);
    tft.setCursor(18, h - 64);
    tft.println(getAlertText(id, true));
    tft.setCursor(18, h - 38);
    tft.println(getAlertText(id, false));
  }
}
