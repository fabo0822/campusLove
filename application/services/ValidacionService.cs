using System;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace campusLove.application.services
{
    public class ValidacionService
    {
        public bool EsCorreoValido(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo)) return false;
            
            try {
                var addr = new MailAddress(correo);
                return addr.Address == correo;
            }
            catch {
                return false;
            }
        }

        public bool EsContrasenaValida(string contrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena)) return false;
            
            return contrasena.Length >= 6 &&
                   contrasena.Any(char.IsUpper) &&
                   contrasena.Any(char.IsLower) &&
                   contrasena.Any(char.IsDigit);
        }

        public bool EsNombreValido(string nombre, int longitudMinima = 3)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return false;
            
            nombre = nombre.Trim();
            return nombre.Length >= longitudMinima &&
                   !nombre.Any(c => char.IsDigit(c)) &&
                   Regex.IsMatch(nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$");
        }

        public bool EsEdadValida(int edad, int edadMinima = 18, int edadMaxima = 99)
        {
            return edad >= edadMinima && edad <= edadMaxima;
        }

        public bool EsGeneroValido(int genero)
        {
            return genero == 1 || genero == 2;
        }

        public bool EsTextoValido(string texto, int longitudMinima)
        {
            if (string.IsNullOrWhiteSpace(texto)) return false;
            
            texto = texto.Trim();
            return texto.Length >= longitudMinima;
        }

        public bool EsCarreraValida(string carrera)
        {
            return EsTextoValido(carrera, 3);
        }

        public bool EsFraseValida(string frase)
        {
            return EsTextoValido(frase, 5);
        }

        public bool SonInteresesValidos(string intereses)
        {
            return EsTextoValido(intereses, 5);
        }
    }
} 