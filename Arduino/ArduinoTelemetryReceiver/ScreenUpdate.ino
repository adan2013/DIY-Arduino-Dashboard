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
