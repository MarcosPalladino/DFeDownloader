# DFeDownloader
  Projeto em dotnet framework 4.5 que faz o download da nota fiscal eletronica a partir do webservice de manifestação do destinatário.
  
  
- Faça o clone do projeto e compile em modo release.
- Copie o arquivo CONFIG.JSON.SAMPLE para o mesmo diretório do arquivo DFeDownloader.exe.
- Renomeie o arquivo config.json.sample para config.json e configure as informações do arquivo.
- Crie uma pasta chamada "Certificado" e adicione dentro o certificado usado para emitir a nota fiscal eletronica com o nome "certificado.pfx"
- Exemplo da estrutura de arquivos na pasta Images.
 
 
  Ao executar o DFeDownloader ele irá usar o certificado digital para estabelecer uma conexão com o webservice de manifestação do destinatário da receita federal e irá solicitar 50 documentos fiscais eletronicos, ao receber irá salvar somente os arquivos com a nota fiscal completa.
  
  A nota fiscal completa só é disponibilizadda para chaves da NFE que possuírem manifestação do destinatário (ao menos a ciencia da operação).

