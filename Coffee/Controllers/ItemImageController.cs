using Application.DTOs.ItemImages;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers
{
    [Route("api/itemimages")]
    [ApiController]
    public class ItemImageController : ControllerBase
    {
        private readonly IItemImageService _itemImageService;
        private readonly ILogger<ItemImageController> _logger;

        public ItemImageController(IItemImageService itemImageService, ILogger<ItemImageController> logger)
        {
            _itemImageService = itemImageService ?? throw new ArgumentNullException(nameof(itemImageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get item image by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ItemImageResponse>> GetImageById(int id)
        {
            try
            {
                var image = await _itemImageService.GetByIdAsync(id);
                if (image == null)
                {
                    return NotFound($"Image with ID {id} not found");
                }
                return Ok(image);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image with ID {ImageId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the image");
            }
        }

        /// <summary>
        /// Get all images for a specific item
        /// </summary>
        [HttpGet("item/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ItemImageResponse>>> GetImagesByItemId(int itemId)
        {
            try
            {
                var images = await _itemImageService.GetByItemIdAsync(itemId);
                return Ok(images);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting images for item {ItemId}", itemId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving images");
            }
        }

        /// <summary>
        /// Get active images for a specific item
        /// </summary>
        [HttpGet("item/{itemId}/active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ItemImageResponse>>> GetActiveImagesByItemId(int itemId)
        {
            try
            {
                var images = await _itemImageService.GetActiveByItemIdAsync(itemId);
                return Ok(images);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active images for item {ItemId}", itemId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving images");
            }
        }

        /// <summary>
        /// Get default image for a specific item
        /// </summary>
        [HttpGet("item/{itemId}/default")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ItemImageResponse>> GetDefaultImageByItemId(int itemId)
        {
            try
            {
                var image = await _itemImageService.GetDefaultByItemIdAsync(itemId);
                if (image == null)
                {
                    return NotFound($"No default image found for item {itemId}");
                }
                return Ok(image);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default image for item {ItemId}", itemId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the default image");
            }
        }

        /// <summary>
        /// Upload a new image for an item (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize] // Add role-based authorization: [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ItemImageResponse>> CreateItemImage([FromBody] CreateItemImageRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var image = await _itemImageService.CreateAsync(request);
                _logger.LogInformation("Image created for item {ItemId}", request.ItemId);

                return CreatedAtAction(nameof(GetImageById), new { id = image.Id }, image);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Image creation failed: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating item image");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the image");
            }
        }

        /// <summary>
        /// Update an item image (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize] // Add role-based authorization: [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ItemImageResponse>> UpdateItemImage(int id, [FromBody] UpdateItemImageRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var image = await _itemImageService.UpdateAsync(id, request);
                if (image == null)
                {
                    return NotFound($"Image with ID {id} not found");
                }

                _logger.LogInformation("Image {ImageId} updated successfully", id);
                return Ok(image);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Image update failed: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating image with ID {ImageId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the image");
            }
        }

        /// <summary>
        /// Set an image as the default for its item (Admin only)
        /// </summary>
        [HttpPut("{id}/set-default")]
        [Authorize] // Add role-based authorization: [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetAsDefault(int id)
        {
            try
            {
                var result = await _itemImageService.SetAsDefaultAsync(id);
                if (!result)
                {
                    return NotFound($"Image with ID {id} not found");
                }

                _logger.LogInformation("Image {ImageId} set as default", id);
                return Ok(new { message = "Image set as default successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting image {ImageId} as default", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while setting the default image");
            }
        }

        /// <summary>
        /// Delete an item image (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize] // Add role-based authorization: [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteItemImage(int id)
        {
            try
            {
                var result = await _itemImageService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound($"Image with ID {id} not found");
                }

                _logger.LogInformation("Image {ImageId} deleted", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image with ID {ImageId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the image");
            }
        }

        /// <summary>
        /// Activate an item image (Admin only)
        /// </summary>
        [HttpPut("{id}/activate")]
        [Authorize] // Add role-based authorization: [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateImage(int id)
        {
            try
            {
                var result = await _itemImageService.ActivateAsync(id);
                if (!result)
                {
                    return NotFound($"Image with ID {id} not found");
                }

                _logger.LogInformation("Image {ImageId} activated", id);
                return Ok(new { message = "Image activated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating image with ID {ImageId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while activating the image");
            }
        }

        /// <summary>
        /// Deactivate an item image (Admin only)
        /// </summary>
        [HttpPut("{id}/deactivate")]
        [Authorize] // Add role-based authorization: [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateImage(int id)
        {
            try
            {
                var result = await _itemImageService.DeactivateAsync(id);
                if (!result)
                {
                    return NotFound($"Image with ID {id} not found");
                }

                _logger.LogInformation("Image {ImageId} deactivated", id);
                return Ok(new { message = "Image deactivated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Image deactivation failed: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating image with ID {ImageId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deactivating the image");
            }
        }
    }
}
