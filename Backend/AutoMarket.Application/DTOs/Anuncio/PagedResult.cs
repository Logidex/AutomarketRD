using System;
using System.Collections.Generic;

namespace AutoMarket.Application.DTOs
{
    // La <T> significa que esta clase puede envolver cualquier tipo de DTO
    public class PagedResult<T>
    {
        public int TotalRegistros { get; set; }
        public int PaginaActual { get; set; }
        public int CantidadPorPagina { get; set; }
        
        // El total de páginas se calcula solo matemáticamente
        public int TotalPaginas => (int)Math.Ceiling(TotalRegistros / (double)CantidadPorPagina);
        
        public IReadOnlyCollection<T> Items { get; set; }

        // Constructor para forzar que siempre se pasen todos los datos necesarios
        public PagedResult(IReadOnlyCollection<T> items, int totalRegistros, int paginaActual, int cantidadPorPagina)
        {
            Items = items;
            TotalRegistros = totalRegistros;
            PaginaActual = paginaActual;
            CantidadPorPagina = cantidadPorPagina;
        }
    }
}