datestring=`date +\'YMd_HMS\'`;
cp Client.Log "Client-${datestring}.Log";
cp Server.Log "Server-${datestring}.Log";
