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
