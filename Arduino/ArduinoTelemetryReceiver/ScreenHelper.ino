void clearLcd() {
  tft.fillScreen(ILI9341_BLACK);
}

void clearCenterScreen() {
  tft.fillRect(0, 52, tft.width(), 190, ILI9341_BLACK);
}

String getAlertText(int id, bool firstLine) {
  switch(id) {
    case 31: return firstLine ? "Brake low" : "pressure!";
    default: return "";
  }
}

uint16_t getDiagnosticColor(int dmg) {
  uint16_t color = ILI9341_DARKGREEN;
  if(dmg >= 3 && dmg < 6) { color = ILI9341_GREEN;
  }else if(dmg >= 6 && dmg < 9) { color = ILI9341_YELLOW;
  }else if(dmg >= 9 && dmg < 12) { color = ILI9341_ORANGE;
  }else if(dmg >= 12) { color = ILI9341_RED; }
  return color;
}

String getAssistantType(String hexId) {
  int id = 0;
  if(hexId == "A") { id = 10;
  }else if(hexId == "B") { id = 11;
  }else if(hexId == "C") { id = 12;
  }else if(hexId == "D") { id = 13;
  }else if(hexId == "E") { id = 14;
  }else if(hexId == "F") { id = 15;
  }else{ id = hexId.toInt(); }
  switch(id) {
    case 1: return "Nav time";
    case 2: return "Nav distance";
    case 3: return "Rest time";
    case 4: return "Job deliv. time";
    case 5: return "Air pressure";
    case 6: return "Oil temp.";
    case 7: return "Oil pressure";
    case 8: return "Water temp.";
    case 9: return "Battery";
    case 10: return "Fuel left";
    case 11: return "Fuel AVG";
    case 12: return "Fuel range";
    case 13: return "Current speed";
    default: return "";
  }
}

String getTruckPartName(int id) {
  switch(id) {
    case 0: return "CAB-";
    case 1: return "CHA-";
    case 2: return "ENG-";
    case 3: return "TRA-";
    case 4: return "WHE-";
    default: return "";
  }
}

void printMenuNavHint(String text) {
  int w = tft.width(), h = tft.height();
  tft.fillRect(0, h - 33, w, 15, ILI9341_BLACK);
  tft.setCursor(w - 20 - text.length() * 6, h - 30);
  tft.setTextSize(1);
  tft.println(text);
}

void printMenuItem(int row, String text, bool selected) {
  if(text == "") return;
  int w = tft.width();
  int offsetY = 62 + row * 26;
  uint16_t bg = ILI9341_BLACK;
  tft.setCursor(36, offsetY + 5);
  if(selected) {
    bg = ILI9341_WHITE;
    tft.setTextColor(ILI9341_BLACK);
  }
  tft.fillRect(0, offsetY, w, 26, bg);
  if(selected) tft.fillTriangle(14, offsetY + 6, 28, offsetY + 13, 14, offsetY + 20, ILI9341_BLACK);
  tft.println(text);
  if(selected) tft.setTextColor(ILI9341_WHITE);
}

void clearParam(int row) {
  int offsetY = 62 + row * 22;
  tft.fillRect(0, offsetY, tft.width(), 22, ILI9341_BLACK);
}

void printParam(int row, String text, bool toRight) {
  if(text == "") return;
  int offsetY = 62 + row * 22;
  if(toRight) {
    tft.setCursor(tft.width() - 35 - text.length() * 10, offsetY);
  }else{
    tft.setCursor(18, offsetY);
  }
  tft.println(text);
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