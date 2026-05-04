using Application.DTOs.Item;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers
{
    [Route("api/item")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly ILogger<ItemController> _logger;

        public ItemController(IItemService itemService, ILogger<ItemController> logger)
        {
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all active items
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ItemResponse>>> GetAllActiveItems()
        {
            try
            {
                var items = await _itemService.GetAllActiveAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all active items");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving items");
            }
        }

        /// <summary>
        /// Get item by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ItemResponse>> GetItemById(int id)
        {
            try
            {
                var item = await _itemService.GetByIdAsync(id);
                if (item == null)
                {
                    return NotFound($"Item with ID {id} not found");
                }
                return Ok(item);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item with ID {ItemId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the item");
            }
        }

        /// <summary>
        /// Get items by category
        /// </summary>
        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ItemResponse>>> GetItemsByCategory(int categoryId)
        {
            try
            {
                var items = await _itemService.GetItemsByCategoryAsync(categoryId);
                return Ok(items);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting items for category {CategoryId}", categoryId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving items");
            }
        }

        /// <summary>
        /// Create a new item (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize] // Add role-based authorization: [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ItemResponse>> CreateItem([FromBody] CreateItemRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var item = await _itemService.CreateAsync(request);
                _logger.LogInformation("Item {ItemName} created successfully", request.Name);

                return CreatedAtAction(nameof(GetItemById), new { id = item.Id }, item);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Item creation failed: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating item");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the item");
            }
        }

        /// <summary>
        /// Update an existing item (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize] // Add role-based authorization: [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ItemResponse>> UpdateItem(int id, [FromBody] UpdateItemRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var item = await _itemService.UpdateAsync(id, request);
                if (item == null)
                {
                    return NotFound($"Item with ID {id} not found");
                }

                _logger.LogInformation("Item {ItemId} updated successfully", id);
                return Ok(item);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Item update failed: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item with ID {ItemId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the item");
            }
        }

        /// <summary>
        /// Delete (deactivate) an item (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize] // Add role-based authorization: [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteItem(int id)
        {
            try
            {
                var result = await _itemService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound($"Item with ID {id} not found");
                }

                _logger.LogInformation("Item {ItemId} deactivated", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item with ID {ItemId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the item");
            }
        }

        /// <summary>
        /// Activate an item (Admin only)
        /// </summary>
        [HttpPut("{id}/activate")]
        [Authorize] // Add role-based authorization: [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateItem(int id)
        {
            try
            {
                var result = await _itemService.ActivateAsync(id);
                if (!result)
                {
                    return NotFound($"Item with ID {id} not found");
                }

                _logger.LogInformation("Item {ItemId} activated", id);
                return Ok(new { message = "Item activated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating item with ID {ItemId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while activating the item");
            }
        }

        /// <summary>
        /// Deactivate an item (Admin only)
        /// </summary>
        [HttpPut("{id}/deactivate")]
        [Authorize] // Add role-based authorization: [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateItem(int id)
        {
            try
            {
                var result = await _itemService.DeactivateAsync(id);
                if (!result)
                {
                    return NotFound($"Item with ID {id} not found");
                }

                _logger.LogInformation("Item {ItemId} deactivated", id);
                return Ok(new { message = "Item deactivated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating item with ID {ItemId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deactivating the item");
            }
        }
    }
}
