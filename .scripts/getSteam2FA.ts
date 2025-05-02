import SteamTotp from 'steam-totp';
console.log(SteamTotp.generateAuthCode(process.argv[2]));