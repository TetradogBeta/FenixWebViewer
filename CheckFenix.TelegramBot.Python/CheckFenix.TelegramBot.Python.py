from Capitulo import Capitulo
from os.path import exists
import time
import sys
import telegram


pathConfig="config";

if exists(pathConfig):
    fConfig = open(pathConfig, "r");
    config = fConfig.readlines();
    fConfig.close();
    pathFile=config[0].replace("\n","");
    uriWeb=config[1].replace("\n","");
    channel=config[2].replace("\n","");
    apiKey=config[3].replace("\n","");
elif sys.argv.len==4:
    pathFile=sys.argv[0];
    uriWeb=sys.argv[1];
    channel=sys.argv[2];
    apiKey=sys.argv[3];
elif sys.argv.len>0:
    raise Exception("Se necesitan más parametros: file,web,channel,apiKey");
else:
    raise Exception("Se necesita la configuración ya sea por parametro o por archivo de file,web,channel,apiKey");




print("CheckFenix V1.0 Telegram bot");
#leo los capitulos ya publicados y los añado al diccionario
capitulosPublicados={};
if exists(pathFile):
    fCapitulos = open(pathFile, "r");
    capitulosGuardados = fCapitulos.readlines();
    fCapitulos.close();
    for capitulo in capitulosGuardados:
        capitulosPublicados[capitulo]=capitulo;


while True:
    for capitulo in Capitulo.GetCapitulos(uriWeb):#me falta invertirlo
        if capitulo.Name not in capitulosPublicados:
            print(capitulo.Name);
            capitulosPublicados[capitulo.Name]=capitulo.Name;
            fCapitulos = open(pathFile, 'a');
            fCapitulos.writelines([capitulo.Name]);
            fCapitulos.close();
    print("Descanso de 5 min");
    time.sleep(5*60);

  
