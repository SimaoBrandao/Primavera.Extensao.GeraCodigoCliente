using System;
using Primavera.Extensibility.Base.Editors;
using StdPlatBS100;
using Primavera.Extensibility.BusinessEntities.ExtensibilityService.EventArgs;

namespace Primavera.Extensao.GeraCodigoCliente.Base
{
    public class UiFichaClientes : FichaClientes
    {
        public override void AntesDeCriar(ExtensibilityEventArgs e)
        {
            try
            {
                //Gera codigo do Cliente preenche a caixa de texto ao abrir o formul�rio e ao clicar no bot�o Novo.
                this.Cliente.Cliente = GerarCodigoCliente();

                base.AntesDeCriar(e);
            }
            catch (Exception ex)
            {
                //Cancel = true;
                PSO.Dialogos.MostraMensagem(
                    StdBSTipos.TipoMsg.PRI_SimplesOk,
                    "Erro ao validar c�digo: " + ex.Message
                );
            }
        }

        public override void AntesDeGravar(ref bool Cancel, ExtensibilityEventArgs e)
        {
            // Se estiver vazio ou duplicado, gera outro
            if (string.IsNullOrEmpty(this.Cliente.Cliente) || BSO.Base.Clientes.Existe(this.Cliente.Cliente))
            {
                string codigoAnterior = this.Cliente.Cliente;
                string codigoCliente = GerarCodigoCliente();
                this.Cliente.Cliente = codigoCliente;

                PSO.Dialogos.MostraMensagem(
                    StdBSTipos.TipoMsg.PRI_SimplesOk,
                    $"O c�digo {codigoAnterior} j� existia. Foi atribu�do: {codigoCliente}"
                );
            }

            base.AntesDeGravar(ref Cancel, e);
        }

        /// <summary>
        /// Gera um c�digo no formato C001, C002, C003... e ajusta dinamicamente o n�mero de d�gitos.
        /// </summary>
        private string GerarCodigoCliente()
        {
            string prefixo = "C";
            int numero = 1;
            string codigo;

            try
            {
                var lista = BSO.Base.Clientes.LstClientes();

                if (lista.NumLinhas() > 0)
                {
                    lista.Inicio();
                    while (!lista.NoFim())
                    {
                        string cod = lista.Valor("Cliente").ToString();

                        if (cod.StartsWith(prefixo))
                        {
                            if (int.TryParse(cod.Substring(prefixo.Length), out int num))
                            {
                                if (num >= numero)
                                    numero = num + 1;
                            }
                        }

                        lista.Seguinte();
                    }
                }

                // N�mero de d�gitos din�mico � nunca menor que 3
                int digitos = Math.Max(3, numero.ToString().Length);
                codigo = $"{prefixo}{numero.ToString().PadLeft(digitos, '0')}";

                // Garante unicidade absoluta
                while (BSO.Base.Clientes.Existe(codigo))
                {
                    numero++;
                    digitos = Math.Max(3, numero.ToString().Length);
                    codigo = $"{prefixo}{numero.ToString().PadLeft(digitos, '0')}";
                }

                return codigo;
            }
            catch
            {
                // fallback: gera um c�digo pseudoaleat�rio seguro
                return $"{prefixo}{DateTime.Now.Ticks % 1000000:D6}";
            }
        }
    }
}
