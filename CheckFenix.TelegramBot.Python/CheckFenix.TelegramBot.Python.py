from Capitulo import Capitulo
from os.path import exists
import numpy
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
elif len(sys.argv)==5:
    pathFile=sys.argv[1];
    uriWeb=sys.argv[2];
    channel=sys.argv[3];
    apiKey=sys.argv[4];
    fConfig = open(pathConfig, 'w');
    fConfig.writelines([pathFile+"\n",uriWeb+"\n",channel+"\n",apiKey+"\n"]);
    fConfig.close();
elif len(sys.argv)>0:
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
        capitulosPublicados[capitulo.replace("\n","")]=capitulo;


while True:
    try:
        for capitulo in numpy.flip(numpy.array(Capitulo.GetCapitulos(uriWeb))):
            if capitulo.Name not in capitulosPublicados:
                print(capitulo.Name);
                capitulosPublicados[capitulo.Name]=capitulo.Name;
                fCapitulos = open(pathFile, 'a');
                fCapitulos.write(capitulo.Name+"\n");
                fCapitulos.close();
                #publico el capitulo
        print("Descanso de 5 min");
        time.sleep(5*60);
    except:
         print("Sin conexión, vuelvo a intentarlo en 10 segundos");
         time.sleep(10);
  
