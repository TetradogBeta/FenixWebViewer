import requests


def GetMessage(chat,idMessage):
    htmlWeb=requests.get(str(chat.link)+"/"+str(idMessage));
    #<meta name="twitter:description" content="Init CheckFenixBot">
    strMeta=htmlWeb.text.split("<meta name=\"twitter:description\"")[1];
    strMeta=strMeta.split("content=\"")[1];
    content= strMeta.split("\"")[0];
    return content;