- Translated for PT BR by: Slugg

Mod Information

- Provides support for custom region spawns in Expedition mode.
- Since no custom regions have support by default, to actually spawn in a region the [ randomstarts.txt ] file needs to be edited directly with room names.
- This mod changes how regions are selected, and the application method is different for no modded regions, modded regions are enabled, and slugbase is enabled.

Region Restrictions: How to use

- Create a folder named [ modify ] in the base folder of your mod.
- Inside that folder create a [ restricted-regions.txt ] file.
- Use the syntax, and formatting specified below to establish restrictions to regions, or rooms.
- Each line must begin with a merge prefix such as [ADD].
- See https://rainworldmodding.miraheze.org/wiki/Downpour_Reference/Modification_Files for more information.

Comments must be on separate lines and are implemented using the double forward slash (//).

- To restrict a region, use a region code prefixed with $
- Use new lines to distinguish between unique region restrictions

$XX
XX restrictions here
$XY
XY restrictions here

Valid formats

$XX, $XX, $XX - Multiple regions can be restricted at once.
$XX, XX, XX

Lines following a region code define what restrictions apply to that region(s) in Expedition.

Data fields are delimited with commas (,), and headers are delimited with (:). Uppercase is preferred for most headers.

Valid formats

Data field values may be formatted in two ways:

HEADER: val1, val2

HEADER:
val1
val2

Data field types

WORLD STATE - Only a slugcat that has access to a certain world state has access.
Valid values: Vanilla, Gourmand, Rivulet, Spearmaster, Artificer, Saint, Other, OldWorld, MSC, Any, None

SLUGCATS - Only the slugcat names listed here will have access, or be prevented from accessing
Include ALLOW/NOTALLOW to specify the behavior. 
Valid values: Any slugcat name, including alias names (White, and Survivor both work)

ROOMS - Rooms designated to have room-specific restrictions applied to them. If no restrictions are defined under the ROOMS header,
the restrictions defined for the region will apply to listed rooms instead.

ProgressionRestriction - This specifies the progression the player must achieve to have access.
This field has no spaces and header must be delimited by its value by a period (.).
Valid values: OnVisit, OnSlugcatUnlocked
The line following an OnSlugcatUnlocked restriction should contain slugcat names delimited by commas (,).
When more than one slugcat is listed here, all listed slugcats must be unlocked.

Instructions:
Adding player spawns to Expedition:
- Create a [ randomstarts.txt ] file inside the modify folder.
- Room codes should be prefixed with [ADD], one room per line. (Use uppercase format for room codes)

Shelters are the preferred spawn areas in Expedition. Player spawns are not only limited to shelters,
as almost any room can serve as a spawn area in Expedition.

Limiting region access until campaign completion for a specific slugcat

The easiest way to handle this is to unlock your slugcat for Expedition play when campaign completion conditions are met.
Establish a ProgressionRestriction.OnSlugcatUnlocked restriction to unlock your region.

Code mod restrictions:
Regions may be restricted using custom logic through a code mod. Though it is preferred that it be handled through a file
whenever possible.

- Your mod will need to have this mod as a project reference to access mod-controlled classes.
- Code restrictions are handled through [ ExpeditionRegionSupport.Regions.RegionUtils.AssignRestriction() ] (subject to change).
- Your restriction will be stored in a RestrictionCheck object, which specifies a region code, and accepts a delegate that
is used to evaluate your region's unlock conditions.
- Optionally you may restrict your region by slugcat unlocked, or to a specific slugcat without requiring a delegate.

-------------------------------------------------------------------------------------------------------------------------

- Traduzido para PT BR por: Slugg

Informações sobre mods

- Fornece suporte para spawns de regiões personalizadas no modo Expedição.
- Como nenhuma região personalizada tem suporte por padrão e para realmente gerar em uma região, o arquivo [ randomstarts.txt ] precisa ser editado diretamente com os nomes das salas.
- Este mod altera a forma como as regiões são selecionadas, e o método de aplicação é diferente para regiões sem modificação, regiões modificadas estão habilitadas e slugbase está habilitado.

Restrições de região: como usar

- Crie uma pasta chamada [ modify ] na pasta base do seu mod.
- Dentro dessa pasta, crie um arquivo [ restricted-regions.txt ].
- Use a sintaxe e a formatação especificadas abaixo para estabelecer restrições a regiões ou salas.
- Cada linha deve começar com um prefixo de mesclagem como [ADD].
- Consulte https://rainworldmodding.miraheze.org/wiki/Downpour_Reference/Modification_Files para obter mais informações.

Os comentários devem estar em linhas separadas e são implementados usando a barra dupla (//).

- Para restringir uma região, use um código de região prefixado com $
- Use novas linhas para distinguir entre restrições de regiões exclusivas

$XX
XX restrições aqui
$XY
XY restrições aqui

Formatos válidos

$XX, $XX, $XX – Várias regiões podem ser restritas ao mesmo tempo.
$XX, XX, XX

As linhas após um código de região definem quais restrições se aplicam a essas regiões na Expedição.

Os campos de dados são delimitados por vírgulas (,) e os cabeçalhos são delimitados por dois pontos (:). Maiúsculos são preferidas para a maioria dos cabeçalhos.

Formatos válidos

Os valores dos campos de dados podem ser formatados de duas maneiras:

CABEÇALHO: val1, val2

CABEÇALHO:
val1
val2

Tipos de campos de dados

WORLD STATE - Somente um slugcat que tem acesso a um determinado estado mundial tem acesso.
Valores válidos: Vanilla, Gourmand, Rivulet, Spearmaster, Artificer, Saint, Other, OldWorld, MSC, Any, None

SLUGCATS - Somente os nomes de slugcat listados aqui terão acesso, ou serão impedidos de acessar
Inclua [ ALLOW/NOTALLOW ] para especificar o comportamento.
Valores válidos: qualquer nome de slugcat, incluindo nomes alternativos (White e Survivor funcionam)

ROOMS - Quartos designados para terem restrições específicas aplicadas a eles. Se nenhuma restrição for definida no cabeçalho ROOMS,
as restrições definidas para a região serão aplicadas aos quartos listados.

ProgressionRestriction - Especifica a progressão que o jogador deve alcançar para ter acesso.
Este campo não possui espaços e o cabeçalho deve ser delimitado pelo seu valor por um ponto (.).
Valores válidos: OnVisit, OnSlugcatUnlocked
A linha após uma restrição OnSlugcatUnlocked deve conter nomes de slugcat delimitados por vírgulas (,).
Quando mais de um slugcat estiver listado aqui, todos os slugcats listados deverão ser desbloqueados.

Instruções:
 Adicionando spawns de jogadores à Expedição:
- Crie um arquivo [ randomstarts.txt ] dentro da pasta [ modify ].
- Os códigos dos quartos devem ser prefixados com [ADD], uma sala por linha. (Use formato maiúsculo para códigos de quarto)

Abrigos são as áreas de desova preferidas na Expedição. A geração de jogadores não se limita apenas a abrigos,
já que quase qualquer sala pode servir como área de spawn na Expedição.

Limitando o acesso à região até a conclusão da campanha para um slugcat específico

A maneira mais fácil de lidar com isso é desbloquear seu slugcat para o jogo Expedition quando as condições de conclusão da campanha forem atendidas.
Estabeleça uma restrição [ ProgressionRestriction.OnSlugcatUnlocked ] para desbloquear sua região.

Restrições de mod de código:
As regiões podem ser restritas usando lógica personalizada por meio de um mod de código. Embora seja preferível que seja tratado através de um arquivo
quando possível.

- Seu mod precisará ter este mod como referência de projeto para acessar classes controladas por mod.
- As restrições de código são tratadas por meio de [ ExpeditionRegionSupport.Regions.RegionUtils.AssignRestriction() ] (sujeito a alterações).
- Sua restrição será armazenada em um objeto RestrictionCheck, que especifica um código de região e aceita um delegado qu é usado para 
avaliar as condições de desbloqueio da sua região.
- Opcionalmente, você pode restringir sua região por slugcat desbloqueado ou para um slugcat específico sem a necessidade de um delegado.