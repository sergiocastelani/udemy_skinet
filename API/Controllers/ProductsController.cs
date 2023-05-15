using API.DTOs;
using API.Errors;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly IGenericRepository<Product> productsRepo;
        private readonly IGenericRepository<ProductBrand> brandsRepo;
        private readonly IGenericRepository<ProductType> typesRepo;

        private readonly IMapper mapper;

        public ProductsController(
            IGenericRepository<Product> productsRepo, 
            IGenericRepository<ProductBrand> brandsRepo, 
            IGenericRepository<ProductType> typesRepo,
            IMapper mapper
        )
        {
            this.typesRepo = typesRepo;
            this.mapper = mapper;
            this.brandsRepo = brandsRepo;
            this.productsRepo = productsRepo;
        }

        [HttpGet]
        public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts(
            [FromQuery] ProductSpecParams productParams
        )
        {
            var spec = new ProductsWithTypesAndBrandsSpecification(productParams);

            var countSpec = new ProductWithFiltersForCountSpecification(productParams);

            var totalItems = await productsRepo.CountAsync(countSpec);

            var products = await productsRepo.ListAsync(spec);

            var data = mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);

            return Ok(new Pagination<ProductToReturnDto>(productParams.PageIndex, productParams.PageSize, totalItems, data));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductToReturnDto>> GetProductById(int id)
        {
            var spec = new ProductsWithTypesAndBrandsSpecification(id);
            var product = await productsRepo.GetEntityWithSpec(spec);
            if (product == null )
                return NotFound(new ApiResponse(404));

            var result = mapper.Map<Product, ProductToReturnDto>(product);

            return Ok(result);
        }

        [HttpGet("brands")]
        public async Task<ActionResult<List<ProductBrand>>> GetProductBrands()
        {
            var brands = await brandsRepo.ListAllAsync();
            return Ok(brands);
        }

        [HttpGet("types")]
        public async Task<ActionResult<List<ProductType>>> GetProductTypes()
        {
            var types = await typesRepo.ListAllAsync();
            return Ok(types);
        }

    }
}