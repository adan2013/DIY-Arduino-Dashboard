void printLcd(int id) {
  screenId = id;
  clearLcd();
  switch(id) {
    case 2: //INITIAL IMAGE
    {
      switch(regB[0].toInt()) {
        case 1: tft.drawBitmap(w / 2 - 40, h / 2 - 45, redarkImage, 90, 90, ILI9341_WHITE); break;
        case 2: tft.drawBitmap(w / 2 - 60, h / 2 - 15, dafImage, 130, 30, ILI9341_WHITE); break;
        case 3: tft.drawBitmap(w / 2 - 60, h / 2 - 15, manImage, 130, 30, ILI9341_WHITE); break;
        case 4: tft.drawBitmap(w / 2 - 40, h / 2 - 45, mercedesImage, 90, 90, ILI9341_WHITE); break;
        case 5: tft.drawBitmap(w / 2 - 40, h / 2 - 45, renaultImage, 90, 90, ILI9341_WHITE); break;
        case 6: tft.drawBitmap(w / 2 - 40, h / 2 - 45, scaniaImage, 90, 90, ILI9341_WHITE); break;
        case 7: tft.drawBitmap(w / 2 - 40, h / 2 - 45, volvoImage, 90, 90, ILI9341_WHITE); break;
        default: tft.drawBitmap(w / 2 - 40, h / 2 - 45, scsImage, 90, 90, ILI9341_WHITE); break;
      }
    }
      break;
    case 3: //ASSISTANT
    {
      tft.setTextColor(ILI9341_LIGHTGREY);
      for(int i = 0; i < 4; i++) printParam(i * 2, getAssistantType(regB[0].substring(i, i + 1)), false);
    }
      break;
    case 4: //NAVIGATION
    {
      tft.setTextColor(ILI9341_LIGHTGREY);
      printParam(0, getAssistantType("1"), false);
      printParam(2, getAssistantType("2"), false);
      printParam(4, getAssistantType("3"), false);
    }
      break;
    case 5: //JOB
    {
      tft.setTextColor(ILI9341_LIGHTGREY);
      printParam(0, getAssistantType("4"), false);
      printParam(2, "Source", false);
      printParam(4, "Destination", false);
    }
      break;
    case 6: //ENGINE
    {
      tft.setTextColor(ILI9341_LIGHTGREY);
      printParam(0, getAssistantType("5"), false);
      printParam(2, "Oil temp/pressure", false);
      printParam(4, getAssistantType("8"), false);
      printParam(6, getAssistantType("9"), false);
    }
      break;
    case 7: //FUEL
    {
      tft.setTextColor(ILI9341_LIGHTGREY);
      printParam(0, "Fuel tank", false);
      printParam(2, getAssistantType("B"), false);
      printParam(4, getAssistantType("C"), false);
      printParam(6, "AdBlue tank", false);
    }
      break;
    case 12: //CUSTOMIZATION
    {
      printParam(1, "Option:", false);
      printParam(4, "Value:", false);
    }
      break;
    case 13: //ACCELERATION
    {
      printParam(3, "Current Speed:", false);
      printParam(5, "Target Speed:", false);
      printParam(7, "Time:", false);
    }
      break;
    case 14: //INFORMATION
    {
      tft.setTextColor(ILI9341_LIGHTGREY);
      printParam(1, "Author:", false);
      printParam(4, "More info:", false);
      tft.setTextColor(ILI9341_WHITE);
      printParam(2, "Daniel Alberski", true);
      printParam(5, "www.redark.pl", true);
      printMenuNavHint("OK-exit");
    }
      break;
    case 15: //TELEMETRY NOT CONNECTED
    {
      tft.setTextSize(2);
      printParam(0, "Telemetry", false);
      printParam(1, "not connected!", false);
    }
      break;
  }
  tft.setTextColor(ILI9341_WHITE);
  updateLcd(true);
}
