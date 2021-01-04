void updateLcd() {
  int w = tft.width(), h = tft.height();
  switch(screenId) {
    case 1: //TESTING
      if(clearValuesRequired) clearLcd();
      tft.fillScreen(regB[0] == "0" ? ILI9341_RED : regB[0] == "1" ? ILI9341_GREEN : regB[0] == "2" ? ILI9341_BLUE : ILI9341_WHITE);
      break;
    case 3: //ASSISTANT
      if(clearValuesRequired) { clearParam(1); clearParam(3); clearParam(5); clearParam(7); }
      printParam(1, regB[1], true);
      printParam(3, regB[2], true);
      printParam(5, regB[3], true);
      printParam(7, regB[4], true);
      break;
  }
  clearValuesRequired = true;
}
