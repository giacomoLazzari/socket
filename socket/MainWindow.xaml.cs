using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Net;
using System.Windows.Threading;
using System.Threading;

namespace socket
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //UTILIZZARE INDIRIZZO 127.0.0.1 (INDIRIZZO DI LOOPBACK)
        //dichiariamo la variabile socket
        Socket socket = null;

        //dichiariamo il dispatcher
        DispatcherTimer dTimer = null;
        List<string> listaNomi;
        Dictionary<string, string> indirizziIPContatti;
        Dictionary<string, string> porteContatti;
        public MainWindow()
        {
            InitializeComponent();

            generaContatti();
            //con addressFamily.internetwork stabilisco che la comunicazione sarà tramite ipv4
            //sockettype.dgram e protocoltype.UDP seleziona di lavorare in UDP
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //abilito il broadcast
            socket.EnableBroadcast = true;

            //IPaddress è un tipo di variabile così come "int"
            //vado a definire un ip  per il mittente
            IPAddress local_address = IPAddress.Any;
            //definisco la porta su cui comunicheremo
            IPEndPoint local_endpoint = new IPEndPoint(local_address.MapToIPv4(), 64500);

            //associo il socket con il local endpoint
            socket.Bind(local_endpoint);

            //socket.Blocking = false;
            //dichiaro il nuovo thread che verrà lanciato sul metodo "thread"
            Thread t1 = new Thread(new ThreadStart(thread));
            //lancio il thread
            t1.Start();
        }


        public void thread()
        {
            int i = 0;
            //imposto "i" uguale a 0 permettendo a questo while di ciclare fino a che il programma è in esecuzione
            while (i==0)
            {
                //imposto il tempo di attesa tra un aggiornamento e l'altro di 250 millisecondi, in questo tempo il thread starà dormendo e in attesa di essere risvegliato per
                // lanciare di nuovo il metodo "aggiornamento_dTimer" che andrà a visualizzare se ci sono dei nuovi messaggi
                Thread.Sleep(TimeSpan.FromMilliseconds(250));
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    aggiornamento_dTimer();

                }));
            }


        }

        private void aggiornamento_dTimer()
        {

            int nBytes = 0;
            nBytes = socket.Available;
            //controllo se è arrivato qualcosa
            if ((nBytes)>0)
            {
                //ricezione dei caratteri in attesa
                byte[] buffer = new byte[nBytes];
                //definiamo l'indirizzo di ricezione
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any,0);
                //prendo dall'header l'indirizzo ip e la porta
                nBytes = socket.ReceiveFrom(buffer, ref remoteEndPoint);
                //fa vedere da dove arrivano i dati
                string from = ((IPEndPoint)remoteEndPoint).Address.ToString();
                //trasformo quello che mi è arrivato in una stringa
                string messaggio = Encoding.UTF8.GetString(buffer, 0, nBytes);

                lstMessaggi.Items.Add(from + messaggio);
            }
        }

        private void btnInvia_Click(object sender, RoutedEventArgs e)
        {
            //definisco l'ip address del destinatario
            IPAddress remote_address = IPAddress.Parse(txtIP.Text);
            //definisco la porta su cui comunicheremo
            IPEndPoint remote_endpoint = new IPEndPoint(remote_address, int.Parse(txtPorta.Text));
            //memorizzo il messaggio da inviare in un array di bytes
            byte[] messaggio = Encoding.UTF8.GetBytes(txtMessaggio.Text);
            //inviamo il messaggio, inserendo messaggio e destinatario.
            socket.SendTo(messaggio, remote_endpoint);

        }

        private void inviaBroadcast()
        {
            //definisco gli ip address dei destinatari
            List<IPAddress> remote_addresses = new List<IPAddress>();
            foreach(string n in listaNomi)
            {
                remote_addresses.Add(IPAddress.Parse(indirizziIPContatti[n]));
            }
            //definisco le porte di ognuno dei destinatari
            List<IPEndPoint> remote_endpoints = new List<IPEndPoint>();
            
            int i = 0;          
            foreach (IPAddress ia in remote_addresses)
            {
               string nome = listaNomi[i];
               remote_endpoints.Add(new IPEndPoint(ia, int.Parse(porteContatti[nome])));
                i++;
            }
            
            //memorizzo il messaggio da inviare in un array di bytes
            byte[] messaggio = Encoding.UTF8.GetBytes(txtMessaggio.Text);
            //inviamo il messaggio ad ogni destinatario
            foreach(IPEndPoint ep in remote_endpoints)
            {
                socket.SendTo(messaggio, ep);
            }
            
        }

        private void generaContatti()
        {
            listaNomi = new List<string>();
            listaNomi.Add("luca");
            listaNomi.Add("susanna");
            listaNomi.Add("matteo");
            listaNomi.Add("giacomo");
            listaNomi.Add("maria");
            lstNomi.ItemsSource = listaNomi;
            indirizziIPContatti = new Dictionary<string, string>();
            indirizziIPContatti.Add("luca", "127.0.0.1");
            indirizziIPContatti.Add("susanna", "127.0.0.1");
            indirizziIPContatti.Add("matteo", "127.0.0.1");
            indirizziIPContatti.Add("giacomo", "127.0.0.1");
            indirizziIPContatti.Add("maria", "127.0.0.1");
            porteContatti = new Dictionary<string, string>();
            porteContatti.Add("luca", "64000");
            porteContatti.Add("susanna", "64001");
            porteContatti.Add("matteo", "64002");
            porteContatti.Add("giacomo", "64003");
            porteContatti.Add("maria", "64004");
        }

        private void lstNomi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string nome = lstNomi.SelectedItem.ToString();
            txtIP.Text = indirizziIPContatti[nome];
            txtPorta.Text = porteContatti[nome];
        }

        private void btnBroadCast_Click(object sender, RoutedEventArgs e)
        {
            inviaBroadcast();
        }
    }
}
